using MacroCreator.Models;
using MacroCreator.Models.Events;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MacroScript.Dsl;

/// <summary>
/// 负责将 DSL 脚本字符串解析为 MacroEvent 列表。
/// </summary>
public partial class DslParser
{
    private int _labelCounter = 0;
    private readonly Stack<FlowControlBlock> _blockStack = new();
    private readonly List<MacroEvent> _events = [];
    private readonly Dictionary<string, bool> _labels = new(StringComparer.OrdinalIgnoreCase);

    // 用于跟踪 Goto 语句，以便稍后验证标签是否存在
    private readonly List<Tuple<JumpEvent, string, int>> _gotoFixups = [];

    // 辅助类，用于跟踪 IF/WHILE 块所需的标签
    private class FlowControlBlock
    {
        public string Type { get; }
        public string? StartLabel { get; } // 用于 while 循环的起点
        public string? ElseLabel { get; }  // 用于 if 的 false 分支或 while 的起点
        public string EndLabel { get; }   // 块的出口点
        public int LineNumber { get; }

        public FlowControlBlock(string type, string? startLabel, string? elseLabel, string endLabel, int lineNumber)
        {
            Type = type;
            StartLabel = startLabel;
            ElseLabel = elseLabel;
            EndLabel = endLabel;
            LineNumber = lineNumber;
        }
    }

    /// <summary>
    /// 解析 DSL 脚本。
    /// </summary>
    public List<MacroEvent> Parse(string dslScript)
    {
        _events.Clear();
        _blockStack.Clear();
        _labels.Clear();
        _gotoFixups.Clear();
        _labelCounter = 0;

        var lines = dslScript.Split(['\n']);

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i].Trim();

            // 忽略注释和空行
            if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                continue;

            try
            {
                ParseLine(line, lineNumber);
            }
            catch (Exception ex) when (ex is not DslParserException)
            {
                // 捕获通用的解析错误
                throw new DslParserException($"行 {lineNumber} 发生内部解析错误: {ex.Message}", lineNumber, ex);
            }
        }

        if (_blockStack.Count > 0)
        {
            var block = _blockStack.Peek();
            throw new DslParserException($"未结束的 '{block.Type}' 块，在行 {block.LineNumber} 定义", block.LineNumber);
        }

        // 验证所有 Goto 标签都存在
        ValidateGotoLabels();

        return _events;
    }

    private string NewLabel(string prefix = "auto") => $"__dsl_{prefix}_{_labelCounter++}";

    private void AddLabel(string labelName, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(labelName) || labelName.StartsWith("__dsl_"))
            throw new DslParserException($"行 {lineNumber}: 无效的标签名称 '{labelName}'。不能使用 '__dsl_' 前缀。", lineNumber);

        if (_labels.ContainsKey(labelName))
            throw new DslParserException($"行 {lineNumber}: 标签 '{labelName}' 已定义。", lineNumber);

        _labels.Add(labelName, true);
        // 添加一个无操作 (NOP) 事件作为标签
        _events.Add(new Nop(labelName));
    }

    private void ValidateGotoLabels()
    {
        foreach (var fixup in _gotoFixups)
        {
            var jumpEvent = fixup.Item1;
            var targetName = fixup.Item2;
            var lineNumber = fixup.Item3;

            if (!_labels.ContainsKey(targetName))
            {
                throw new DslParserException($"行 {lineNumber}: 未找到 'Goto' 语句的目标标签 '{targetName}'", lineNumber);
            }
            // 标签存在，设置跳转目标
            jumpEvent.TargetEventName = targetName;
        }
    }

    /// <summary>
    /// 解析单行 DSL。
    /// </summary>
    private void ParseLine(string line, int lineNumber)
    {
        string command = line.Split(['(', ' '], 2)[0].Trim().ToLowerInvariant();

        switch (command)
        {
            // --- 流程控制 ---
            case "if":
                HandleIf(line, lineNumber);
                break;
            case "else":
                HandleElse(lineNumber);
                break;
            case "endif":
                HandleEndIf(lineNumber);
                break;
            case "while":
                HandleWhile(line, lineNumber);
                break;
            case "endwhile":
                HandleEndWhile(lineNumber);
                break;
            case "break":
                HandleBreak(lineNumber);
                break;
            case "label":
                HandleLabel(line, lineNumber);
                break;
            case "goto":
                HandleGoto(line, lineNumber);
                break;
            case "exit":
                _events.Add(new BreakEvent { TimeSinceLastEvent = 0 });
                break;

            // --- 原子事件 ---
            case "delay":
                HandleDelay(line, lineNumber);
                break;
            case "mouse":
                HandleMouse(line, lineNumber);
                break;
            case "key":
                HandleKey(line, lineNumber);
                break;

            default:
                throw new DslParserException($"行 {lineNumber}: 未知命令 '{command}'", lineNumber);
        }
    }

    #region 流程控制解析

    private void HandleIf(string line, int lineNumber)
    {
        var (conditionEvent, isNotEquals) = ParseCondition(line, lineNumber);

        string trueLabel = NewLabel("if_true");
        string elseLabel = NewLabel("if_else");
        string endLabel = NewLabel("if_end");

        // 根据操作符 (== 或 !=) 设置跳转目标
        conditionEvent.TrueTargetEventName = isNotEquals ? elseLabel : trueLabel;
        conditionEvent.FalseTargetEventName = isNotEquals ? trueLabel : elseLabel;

        _events.Add(conditionEvent);

        // 为 true 块添加标签
        _events.Add(new Nop(trueLabel));

        // 推入堆栈，以便 else/endif 知道标签
        _blockStack.Push(new FlowControlBlock("if", null, elseLabel, endLabel, lineNumber));
    }

    private void HandleElse(int lineNumber)
    {
        if (_blockStack.Count == 0 || _blockStack.Peek().Type != "if")
            throw new DslParserException($"行 {lineNumber}: 意外的 'else'，没有匹配的 'if'", lineNumber);

        var ifBlock = _blockStack.Pop();

        // 1. 添加一个 GOTO 跳过 else 块
        _events.Add(new JumpEvent { TargetEventName = ifBlock.EndLabel, TimeSinceLastEvent = 0 });

        // 2. 添加 else 标签
        if (string.IsNullOrEmpty(ifBlock.ElseLabel))
            throw new DslParserException($"行 {lineNumber}: 内部错误：If 块缺少 else 标签。", lineNumber);
        _events.Add(new Nop(ifBlock.ElseLabel));

        // 3. 推入一个 "else" 块
        _blockStack.Push(new FlowControlBlock("else", null, null, ifBlock.EndLabel, lineNumber));
    }

    private void HandleEndIf(int lineNumber)
    {
        if (_blockStack.Count == 0 || _blockStack.Peek().Type != "if" && _blockStack.Peek().Type != "else")
            throw new DslParserException($"行 {lineNumber}: 意外的 'endif'，没有匹配的 'if' 或 'else'", lineNumber);

        var block = _blockStack.Pop();

        // 如果是 'if' 块（没有 else），我们需要添加 else 标签
        if (block.Type == "if")
        {
            if (string.IsNullOrEmpty(block.ElseLabel))
                throw new DslParserException($"行 {lineNumber}: 内部错误：If 块缺少 else 标签。", lineNumber);
            _events.Add(new Nop(block.ElseLabel));
        }

        // 添加 endif 标签
        if (string.IsNullOrEmpty(block.EndLabel))
            throw new DslParserException($"行 {lineNumber}: 内部错误：If/Else 块缺少结束标签。", lineNumber);
        _events.Add(new Nop(block.EndLabel));
    }

    private void HandleWhile(string line, int lineNumber)
    {
        var (conditionEvent, isNotEquals) = ParseCondition(line, lineNumber);

        string startLabel = NewLabel("while_start");
        string bodyLabel = NewLabel("while_body");
        string endLabel = NewLabel("while_end");

        // 1. 添加循环开始（检查条件）的标签
        _events.Add(new Nop(startLabel));

        // 2. 添加条件跳转
        conditionEvent.TrueTargetEventName = isNotEquals ? endLabel : bodyLabel;
        conditionEvent.FalseTargetEventName = isNotEquals ? bodyLabel : endLabel;
        _events.Add(conditionEvent);

        // 3. 添加循环体开始的标签
        _events.Add(new Nop(bodyLabel));

        // 4. 推入堆栈
        _blockStack.Push(new FlowControlBlock("while", startLabel, null, endLabel, lineNumber));
    }

    private void HandleEndWhile(int lineNumber)
    {
        if (_blockStack.Count == 0 || _blockStack.Peek().Type != "while")
            throw new DslParserException($"行 {lineNumber}: 意外的 'endwhile'，没有匹配的 'while'", lineNumber);

        var whileBlock = _blockStack.Pop();

        // 1. 添加 GOTO 跳回循环开始处
        if (string.IsNullOrEmpty(whileBlock.StartLabel))
            throw new DslParserException($"行 {lineNumber}: 内部错误：While 块缺少开始标签。", lineNumber);

        _events.Add(new JumpEvent { TargetEventName = whileBlock.StartLabel, TimeSinceLastEvent = 0 });

        // 2. 添加循环结束标签
        _events.Add(new Nop(whileBlock.EndLabel));
    }

    private void HandleBreak(int lineNumber)
    {
        // 在栈中查找最近的 while 循环块
        FlowControlBlock? whileBlock = null;
        foreach (var block in _blockStack)
        {
            if (block.Type == "while")
            {
                whileBlock = block;
                break;
            }
        }

        if (whileBlock == null)
            throw new DslParserException($"行 {lineNumber}: 'break' 只能在 'while' 循环内使用", lineNumber);

        // 添加跳转到循环结束的 JumpEvent
        _events.Add(new JumpEvent { TargetEventName = whileBlock.EndLabel, TimeSinceLastEvent = 0 });
    }

    private void HandleLabel(string line, int lineNumber)
    {
        var match = Regex_Label().Match(line);
        if (!match.Success)
            throw new DslParserException($"行 {lineNumber}: Label 语法无效。应为: Label(MyLabelName)", lineNumber);

        string labelName = match.Groups[1].Value.Trim();
        AddLabel(labelName, lineNumber);
    }

    private void HandleGoto(string line, int lineNumber)
    {
        var match = Regex_Goto().Match(line);
        if (!match.Success)
            throw new DslParserException($"行 {lineNumber}: Goto 语法无效。应为: Goto(MyLabelName)", lineNumber);

        string targetName = match.Groups[1].Value.Trim();
        var jumpEvent = new JumpEvent { TimeSinceLastEvent = 0 };

        // 我们还不知道标签是否存在，先注册一个修复请求
        _gotoFixups.Add(Tuple.Create(jumpEvent, targetName, lineNumber));
        _events.Add(jumpEvent);
    }


    #endregion

    #region 原子事件解析

    private void HandleDelay(string line, int lineNumber)
    {
        var match = Regex_Delay().Match(line);
        if (!match.Success)
            throw new DslParserException($"行 {lineNumber}: Delay 语法无效。应为: Delay(milliseconds)", lineNumber);

        if (!double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double delay))
            throw new DslParserException($"行 {lineNumber}: 无效的延迟毫秒数 '{match.Groups[1].Value}'", lineNumber);

        _events.Add(new DelayEvent { DelayMilliseconds = (int)delay, TimeSinceLastEvent = 0 });
    }

    private void HandleMouse(string line, int lineNumber)
    {
        // 格式: Mouse(Action, X, Y, [WheelDelta], [DelayMs])
        var match = Regex_Mouse().Match(line);
        if (!match.Success)
            throw new DslParserException($"行 {lineNumber}: Mouse 语法无效。", lineNumber);

        var args = SplitArgs(match.Groups[1].Value);
        if (args.Length < 3)
            throw new DslParserException($"行 {lineNumber}: Mouse 事件至少需要3个参数 (Action, X, Y)。", lineNumber);

        if (!Enum.TryParse(args[0], true, out MouseAction action))
            throw new DslParserException($"行 {lineNumber}: 未知的 MouseAction '{args[0]}'", lineNumber);

        if (!int.TryParse(args[1], out int x))
            throw new DslParserException($"行 {lineNumber}: 无效的 X 坐标 '{args[1]}'", lineNumber);

        if (!int.TryParse(args[2], out int y))
            throw new DslParserException($"行 {lineNumber}: 无效的 Y 坐标 '{args[2]}'", lineNumber);

        int wheelDelta = 0;
        double delayMs = 0;

        if (action == MouseAction.Wheel)
        {
            if (args.Length < 4)
                throw new DslParserException($"行 {lineNumber}: Mouse(Wheel) 需要4个参数 (Action, X, Y, WheelDelta)。", lineNumber);
            if (!int.TryParse(args[3], out wheelDelta))
                throw new DslParserException($"行 {lineNumber}: 无效的 WheelDelta '{args[3]}'", lineNumber);
            if (args.Length > 4 && !double.TryParse(args[4], NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw new DslParserException($"行 {lineNumber}: 无效的延迟值 '{args[4]}'", lineNumber);
        }
        else
        {
            if (args.Length > 3 && !double.TryParse(args[3], NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw new DslParserException($"行 {lineNumber}: 无效的延迟值 '{args[3]}'", lineNumber);
        }

        _events.Add(new MouseEvent
        {
            Action = action,
            X = x,
            Y = y,
            WheelDelta = wheelDelta,
            TimeSinceLastEvent = delayMs
        });
    }

    private void HandleKey(string line, int lineNumber)
    {
        // 格式: Key(Action, KeyName, [DelayMs])
        var match = Regex_Key().Match(line);
        if (!match.Success)
            throw new DslParserException($"行 {lineNumber}: Key 语法无效。", lineNumber);

        var args = SplitArgs(match.Groups[1].Value);
        if (args.Length < 2)
            throw new DslParserException($"行 {lineNumber}: Key 事件至少需要2个参数 (Action, KeyName)。", lineNumber);

        if (!Enum.TryParse(args[0], true, out KeyboardAction action))
            throw new DslParserException($"行 {lineNumber}: 未知的 KeyboardAction '{args[0]}'", lineNumber);

        if (!Enum.TryParse(args[1], true, out Keys key))
            throw new DslParserException($"行 {lineNumber}: 未知的 Keys 枚举值 '{args[1]}'", lineNumber);

        double delayMs = 0;
        if (args.Length > 2 && !double.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
            throw new DslParserException($"行 {lineNumber}: 无效的延迟值 '{args[2]}'", lineNumber);

        _events.Add(new KeyboardEvent
        {
            Action = action,
            Key = key,
            TimeSinceLastEvent = delayMs
        });
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 解析 IF 或 WHILE 后的条件语句。
    /// </summary>
    /// <returns>返回 (ConditionalJumpEvent, isNotEquals) 元组。</returns>
    private static (ConditionalJumpEvent, bool) ParseCondition(string line, int lineNumber)
    {
        var match = Regex_IfWhile().Match(line);
        if (!match.Success)
            throw new DslParserException($"行 {lineNumber}: 无效的条件语法。", lineNumber);

        string conditionBody = match.Groups[2].Value.Trim();
        var conditionalJump = new ConditionalJumpEvent { TimeSinceLastEvent = 0 };
        bool isNotEquals = false;

        // 1. 尝试解析 PixelColor
        // 示例: PixelColor(10, 20) == RGB(255, 0, 0, 5)
        var pixelMatch = Regex_PixelColor().Match(conditionBody);

        if (pixelMatch.Success)
        {
            conditionalJump.ConditionType = ConditionType.PixelColor;
            conditionalJump.X = int.Parse(pixelMatch.Groups["x"].Value);
            conditionalJump.Y = int.Parse(pixelMatch.Groups["y"].Value);

            int r = int.Parse(pixelMatch.Groups["r"].Value);
            int g = int.Parse(pixelMatch.Groups["g"].Value);
            int b = int.Parse(pixelMatch.Groups["b"].Value);
            conditionalJump.ExpectedColor = Color.FromArgb(r, g, b).ToArgb();

            conditionalJump.PixelTolerance = byte.TryParse(pixelMatch.Groups["t"].Value, out byte t) ? t : (byte)0;
            isNotEquals = pixelMatch.Groups["op"].Value == "!=";
        }
        // 2. 尝试解析 Custom
        // 示例: Custom(`hour > 9`)
        else
        {
            var customMatch = Regex_Custom().Match(conditionBody);
            if (customMatch.Success)
            {
                conditionalJump.ConditionType = ConditionType.CustomExpression;
                conditionalJump.CustomCondition = customMatch.Groups["expr"].Value;
                // 对于 Custom，我们假定 "==" (isNotEquals = false)
            }
            else
            {
                throw new DslParserException($"行 {lineNumber}: 无法解析条件 '{conditionBody}'。请使用 PixelColor(...) == RGB(...) 或 Custom(`...`)。", lineNumber);
            }
        }

        return (conditionalJump, isNotEquals);
    }

    /// <summary>
    /// 分割参数字符串，忽略逗号前后的空格。
    /// </summary>
    private static string[] SplitArgs(string argsString)
    {
        return [.. argsString.Split(',').Select(s => s.Trim())];
    }

    [GeneratedRegex(@"PixelColor\s*\(\s*(?<x>\d+)\s*,\s*(?<y>\d+)\s*\)\s*(?<op>==|!=)\s*RGB\s*\(\s*(?<r>\d+)\s*,\s*(?<g>\d+)\s*,\s*(?<b>\d+)\s*(?:,\s*(?<t>\d+)\s*)?\)")]
    private static partial Regex Regex_PixelColor();

    [GeneratedRegex(@"Custom\s*\(\s*`(?<expr>[^`]*)`\s*\)")]
    private static partial Regex Regex_Custom();

    [GeneratedRegex(@"(if|while)\s*\((.+)\)")]
    private static partial Regex Regex_IfWhile();

    [GeneratedRegex(@"Delay\s*\((\s*[\d.]+)\s*\)")]
    private static partial Regex Regex_Delay();

    [GeneratedRegex(@"Key\s*\(([^)]+)\)")]
    private static partial Regex Regex_Key();

    [GeneratedRegex(@"Mouse\s*\(([^)]+)\)")]
    private static partial Regex Regex_Mouse();

    [GeneratedRegex(@"goto\s+([a-zA-Z0-9_]+)")]
    private static partial Regex Regex_Goto();

    [GeneratedRegex(@"label\s+([a-zA-Z0-9_]+)")]
    private static partial Regex Regex_Label();

    #endregion
}
