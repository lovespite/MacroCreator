using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroScript.Dsl;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MacroScript.Dsl;


// --- 语法分析 (Parser) ---

/// <summary>
/// 语法分析器，将 Token 序列转换为 MacroEvent 列表
/// </summary>
public class NewDslParser
{
    private List<Token> _tokens = null!;
    private int _currentTokenIndex;
    private readonly List<MacroEvent> _events = new();
    private int _labelCounter = 0;
    private readonly Stack<FlowControlBlock> _blockStack = new();
    private readonly Dictionary<string, bool> _labelsDefined = new(System.StringComparer.OrdinalIgnoreCase);
    private readonly List<(JumpEvent Event, string TargetName, int LineNumber)> _gotoFixups = new();
    private readonly List<(ConditionalJumpEvent Event, string? TrueTargetName, string? FalseTargetName, int LineNumber)> _conditionalJumpFixups = new();

    private class FlowControlBlock
    {
        public TokenType Type { get; }
        public string? StartLabel { get; }
        public string? ElseLabel { get; }
        public string EndLabel { get; }
        public int LineNumber { get; }

        public FlowControlBlock(TokenType type, string? startLabel, string? elseLabel, string endLabel, int lineNumber)
        {
            Type = type;
            StartLabel = startLabel;
            ElseLabel = elseLabel;
            EndLabel = endLabel;
            LineNumber = lineNumber;
        }
    }

    public List<MacroEvent> Parse(List<Token> tokens)
    {
        _tokens = tokens;
        _currentTokenIndex = 0;
        _events.Clear();
        _labelCounter = 0;
        _blockStack.Clear();
        _labelsDefined.Clear();
        _gotoFixups.Clear();
        _conditionalJumpFixups.Clear();

        while (!IsAtEnd())
        {
            ParseStatement();
            // 跳过语句后的换行符
            while (CurrentToken().Type == TokenType.EndOfLine)
            {
                Consume();
            }
        }

        if (_blockStack.Count > 0)
        {
            var block = _blockStack.Peek();
            throw CreateException($"文件末尾找到未结束的 '{block.Type}' 块，在行 {block.LineNumber} 定义", block.LineNumber);
        }

        ResolveJumps();

        return _events;
    }

    private void ParseStatement()
    {
        var token = CurrentToken();
        if (token.Type == TokenType.EndOfLine) // 跳过空行
        {
            Consume();
            return;
        }

        switch (token.Type)
        {
            case TokenType.KeywordIf: ParseIfStatement(); break;
            case TokenType.KeywordElse: ParseElseStatement(); break;
            case TokenType.KeywordEndIf: ParseEndIfStatement(); break;
            case TokenType.KeywordWhile: ParseWhileStatement(); break;
            case TokenType.KeywordEndWhile: ParseEndWhileStatement(); break;
            case TokenType.KeywordBreak: ParseBreakStatement(); break;
            case TokenType.KeywordLabel: ParseLabelStatement(); break;
            case TokenType.KeywordGoto: ParseGotoStatement(); break;
            case TokenType.KeywordExit: ParseExitStatement(); break;
            case TokenType.KeywordDelay: ParseDelayStatement(); break;
            case TokenType.KeywordMouse: ParseMouseStatement(); break;
            case TokenType.KeywordKey: ParseKeyStatement(); break;
            // EndOfFile 是合法的结束，不应视为错误
            case TokenType.EndOfFile: break;
            default:
                throw CreateException($"意外的 Token '{token.Value}' ({token.Type})，期望一个语句的开始");
        }
        // 确保语句结束（换行或文件尾）
        //if (CurrentToken().Type != TokenType.EndOfLine && CurrentToken().Type != TokenType.EndOfFile)
        //{
        //      throw CreateException($"语句后缺少换行符或文件结束");
        //}
    }

    // --- 语句解析方法 ---

    private void ParseIfStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordIf); // if
        Expect(TokenType.ParenOpen, "'if' 后面需要 '('");
        var (conditionEvent, jumpTargets) = ParseCondition(); // (condition)
        Expect(TokenType.ParenClose, "条件后需要 ')'");
        // 语句结束检查
        ExpectEndOfStatement("'if' 语句后");

        string trueLabel = NewLabel("if_true");
        string elseLabel = NewLabel("if_else");
        string endLabel = NewLabel("if_end");

        // 设置跳转目标，需要考虑 != 操作符
        conditionEvent.TrueTargetEventName = jumpTargets.IsNotEquals ? elseLabel : trueLabel;
        conditionEvent.FalseTargetEventName = jumpTargets.IsNotEquals ? trueLabel : elseLabel;
        _events.Add(conditionEvent);

        // 添加用于解析跳转目标的 Nop 事件
        AddLabelEvent(trueLabel);

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordIf, null, elseLabel, endLabel, line));
    }

    private void ParseElseStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordElse);
        ExpectEndOfStatement("'else' 关键字后");

        if (_blockStack.Count == 0 || _blockStack.Peek().Type != TokenType.KeywordIf)
            throw CreateException($"意外的 'else'，没有匹配的 'if'");

        var ifBlock = _blockStack.Pop();

        // 1. 添加跳转到 endif 的 GOTO
        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, ifBlock.EndLabel, line));
        _events.Add(jumpToEnd);

        // 2. 添加 else 标签
        if (string.IsNullOrEmpty(ifBlock.ElseLabel)) throw CreateException($"内部错误: If 块缺少 else 标签", line);
        AddLabelEvent(ifBlock.ElseLabel);

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordElse, null, null, ifBlock.EndLabel, line));
    }

    private void ParseEndIfStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordEndIf);
        ExpectEndOfStatement("'endif' 关键字后");

        if (_blockStack.Count == 0 || (CurrentBlockType() != TokenType.KeywordIf && CurrentBlockType() != TokenType.KeywordElse))
            throw CreateException($"意外的 'endif'，没有匹配的 'if' 或 'else'");

        var block = _blockStack.Pop();

        // 如果是 if 块（没有 else），需要添加 else 标签占位符
        if (block.Type == TokenType.KeywordIf)
        {
            if (string.IsNullOrEmpty(block.ElseLabel)) throw CreateException($"内部错误: If 块缺少 else 标签", line);
            AddLabelEvent(block.ElseLabel);
        }

        AddLabelEvent(block.EndLabel);
    }

    private void ParseWhileStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordWhile);
        Expect(TokenType.ParenOpen, "'while' 后面需要 '('");
        var (conditionEvent, jumpTargets) = ParseCondition();
        Expect(TokenType.ParenClose, "条件后需要 ')'");
        ExpectEndOfStatement("'while' 语句后");

        string startLabel = NewLabel("while_start");
        string bodyLabel = NewLabel("while_body");
        string endLabel = NewLabel("while_end");

        AddLabelEvent(startLabel);

        // 设置跳转目标，考虑 !=
        conditionEvent.TrueTargetEventName = jumpTargets.IsNotEquals ? endLabel : bodyLabel;
        conditionEvent.FalseTargetEventName = jumpTargets.IsNotEquals ? bodyLabel : endLabel;
        _events.Add(conditionEvent);

        AddLabelEvent(bodyLabel);

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordWhile, startLabel, null, endLabel, line));
    }

    private void ParseEndWhileStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordEndWhile);
        ExpectEndOfStatement("'endwhile' 关键字后");

        if (_blockStack.Count == 0 || CurrentBlockType() != TokenType.KeywordWhile)
            throw CreateException($"意外的 'endwhile'，没有匹配的 'while'");

        var block = _blockStack.Pop();
        if (string.IsNullOrEmpty(block.StartLabel)) throw CreateException($"内部错误: While 块缺少 start 标签", line);

        // 添加跳回循环开始的 GOTO
        var jumpToStart = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToStart, block.StartLabel, line));
        _events.Add(jumpToStart);

        AddLabelEvent(block.EndLabel);
    }

    private void ParseBreakStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordBreak);
        ExpectEndOfStatement("'break' 关键字后");

        var whileBlock = _blockStack.FirstOrDefault(b => b.Type == TokenType.KeywordWhile);
        if (whileBlock == null)
            throw CreateException($"'break' 只能在 'while' 循环内使用");

        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, whileBlock.EndLabel, line));
        _events.Add(jumpToEnd);
    }

    private void ParseLabelStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordLabel);
        // Expect(TokenType.ParenOpen, "'label' 后面需要 '('");
        var labelToken = Expect(TokenType.Identifier, "需要标签名称");
        // Expect(TokenType.ParenClose, "标签名称后需要 ')'");
        ExpectEndOfStatement("'label' 语句后");

        string labelName = labelToken.Value;
        if (labelName.StartsWith("__dsl_"))
            throw CreateException($"无效的标签名称 '{labelName}'。不能使用 '__dsl_' 前缀。", line);
        if (_labelsDefined.ContainsKey(labelName))
            throw CreateException($"标签 '{labelName}' 已定义。", line);

        _labelsDefined.Add(labelName, true);
        _events.Add(new Nop(labelName));
    }

    private void ParseGotoStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordGoto);
        // Expect(TokenType.ParenOpen, "'goto' 后面需要 '('");
        var labelToken = Expect(TokenType.Identifier, "需要目标标签名称");
        // Expect(TokenType.ParenClose, "标签名称后需要 ')'");
        ExpectEndOfStatement("'goto' 语句后");

        var jumpEvent = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpEvent, labelToken.Value, line));
        _events.Add(jumpEvent);
    }

    private void ParseExitStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordExit);
        ExpectEndOfStatement("'exit' 关键字后");
        _events.Add(new BreakEvent { TimeSinceLastEvent = 0 });
    }

    private void ParseDelayStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordDelay);
        Expect(TokenType.ParenOpen, "'Delay' 后面需要 '('");
        var numberToken = Expect(TokenType.Number, "需要延迟的毫秒数");
        Expect(TokenType.ParenClose, "毫秒数后需要 ')'");
        ExpectEndOfStatement("'Delay' 语句后");

        if (!double.TryParse(numberToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double delay))
            throw CreateException($"无效的延迟毫秒数 '{numberToken.Value}'");

        _events.Add(new DelayEvent { DelayMilliseconds = (int)delay, TimeSinceLastEvent = 0 });
    }

    private void ParseMouseStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordMouse);
        Expect(TokenType.ParenOpen, "'Mouse' 后面需要 '('");

        var actionToken = Expect(TokenType.Identifier, "需要 MouseAction (如 LeftDown, Move)");
        if (!System.Enum.TryParse<MouseAction>(actionToken.Value, true, out var action))
            throw CreateException($"未知的 MouseAction '{actionToken.Value}'");

        Expect(TokenType.Comma, "MouseAction 后需要 ','");
        var xToken = Expect(TokenType.Number, "需要 X 坐标");
        if (!int.TryParse(xToken.Value, out int x)) throw CreateException($"无效的 X 坐标 '{xToken.Value}'");

        Expect(TokenType.Comma, "X 坐标后需要 ','");
        var yToken = Expect(TokenType.Number, "需要 Y 坐标");
        if (!int.TryParse(yToken.Value, out int y)) throw CreateException($"无效的 Y 坐标 '{yToken.Value}'");

        int wheelDelta = 0;
        double delayMs = 0;

        if (action == MouseAction.Wheel)
        {
            Expect(TokenType.Comma, "Y 坐标后需要 ',' (对于 Wheel)");
            var deltaToken = Expect(TokenType.Number, "需要 WheelDelta");
            if (!int.TryParse(deltaToken.Value, out wheelDelta)) throw CreateException($"无效的 WheelDelta '{deltaToken.Value}'");
        }

        // 可选的延迟参数
        if (CurrentToken().Type == TokenType.Comma)
        {
            Consume(); // ,
            var delayToken = Expect(TokenType.Number, "需要延迟毫秒数");
            if (!double.TryParse(delayToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw CreateException($"无效的延迟毫秒数 '{delayToken.Value}'");
        }

        Expect(TokenType.ParenClose, "参数列表后需要 ')'");
        ExpectEndOfStatement("'Mouse' 语句后");

        _events.Add(new MouseEvent
        {
            Action = action,
            X = x,
            Y = y,
            WheelDelta = wheelDelta,
            TimeSinceLastEvent = delayMs
        });
    }

    private void ParseKeyStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordKey);
        Expect(TokenType.ParenOpen, "'Key' 后面需要 '('");

        var actionToken = Expect(TokenType.Identifier, "需要 KeyboardAction (如 KeyDown, KeyUp)");
        if (!System.Enum.TryParse<KeyboardAction>(actionToken.Value, true, out var action))
            throw CreateException($"未知的 KeyboardAction '{actionToken.Value}'");

        Expect(TokenType.Comma, "KeyboardAction 后需要 ','");
        var keyToken = Expect(TokenType.Identifier, "需要 Keys 枚举值 (如 LControlKey, A)");
        if (!System.Enum.TryParse<System.Windows.Forms.Keys>(keyToken.Value, true, out var key))
            throw CreateException($"未知的 Keys 枚举值 '{keyToken.Value}'");

        double delayMs = 0;
        // 可选的延迟参数
        if (CurrentToken().Type == TokenType.Comma)
        {
            Consume(); // ,
            var delayToken = Expect(TokenType.Number, "需要延迟毫秒数");
            if (!double.TryParse(delayToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw CreateException($"无效的延迟毫秒数 '{delayToken.Value}'");
        }

        Expect(TokenType.ParenClose, "参数列表后需要 ')'");
        ExpectEndOfStatement("'Key' 语句后");

        _events.Add(new KeyboardEvent
        {
            Action = action,
            Key = key,
            TimeSinceLastEvent = delayMs
        });
    }

    // --- 条件解析 ---
    private (ConditionalJumpEvent conditionEvent, JumpTargets jumpTargets) ParseCondition()
    {
        var conditionEvent = new ConditionalJumpEvent { TimeSinceLastEvent = 0 };
        bool isNotEquals = false;

        if (CurrentToken().Type == TokenType.KeywordPixelColor)
        {
            ParsePixelColorCondition(conditionEvent, out isNotEquals);
        }
        else if (CurrentToken().Type == TokenType.KeywordCustom)
        {
            ParseCustomCondition(conditionEvent);
            isNotEquals = false; // Custom 条件默认为 == true
        }
        else
        {
            throw CreateException($"期望条件（PixelColor 或 Custom），但得到 '{CurrentToken().Value}'");
        }

        return (conditionEvent, new JumpTargets(isNotEquals));
    }

    // 用于传递 isNotEquals 标志
    private record struct JumpTargets(bool IsNotEquals);


    private void ParsePixelColorCondition(ConditionalJumpEvent conditionEvent, out bool isNotEquals)
    {
        Consume(TokenType.KeywordPixelColor);
        Expect(TokenType.ParenOpen, "'PixelColor' 后面需要 '('");
        var xToken = Expect(TokenType.Number, "需要 X 坐标");
        if (!int.TryParse(xToken.Value, out int x)) throw CreateException($"无效的 X 坐标 '{xToken.Value}'");
        Expect(TokenType.Comma, "X 坐标后需要 ','");
        var yToken = Expect(TokenType.Number, "需要 Y 坐标");
        if (!int.TryParse(yToken.Value, out int y)) throw CreateException($"无效的 Y 坐标 '{yToken.Value}'");
        Expect(TokenType.ParenClose, "Y 坐标后需要 ')'");

        var operatorToken = Consume();
        if (operatorToken.Type == TokenType.OperatorEquals) isNotEquals = false;
        else if (operatorToken.Type == TokenType.OperatorNotEquals) isNotEquals = true;
        else throw CreateException($"PixelColor 条件需要 '==' 或 '!=' 操作符");

        Color color;
        byte tolerance = 0;
        if (CurrentToken().Type == TokenType.KeywordRGB)
        {
            Consume(); // RGB
            Expect(TokenType.ParenOpen, "'RGB' 后面需要 '('");
            var rToken = Expect(TokenType.Number, "需要 R 值");
            Expect(TokenType.Comma, "R 值后需要 ','");
            var gToken = Expect(TokenType.Number, "需要 G 值");
            Expect(TokenType.Comma, "G 值后需要 ','");
            var bToken = Expect(TokenType.Number, "需要 B 值");

            // 可选的容差
            if (CurrentToken().Type == TokenType.Comma)
            {
                Consume(); // ,
                var toleranceToken = Expect(TokenType.Number, "需要容差值 (0-255)");
                if (!byte.TryParse(toleranceToken.Value, out tolerance)) throw CreateException($"无效的容差值 '{toleranceToken.Value}'");
            }

            Expect(TokenType.ParenClose, "RGB 参数后需要 ')'");

            if (!int.TryParse(rToken.Value, out int r) || !int.TryParse(gToken.Value, out int g) || !int.TryParse(bToken.Value, out int b))
                throw CreateException("无效的 RGB 颜色值");
            color = System.Drawing.Color.FromArgb(r, g, b);

        }
        // else if (CurrentToken().Type == TokenType.KeywordARGB) ... // 类似地处理 ARGB
        else
        {
            throw CreateException($"期望 'RGB(...)'"); // 或 ARGB
        }

        conditionEvent.ConditionType = ConditionType.PixelColor;
        conditionEvent.X = x;
        conditionEvent.Y = y;
        conditionEvent.ExpectedColor = color.ToArgb();
        conditionEvent.PixelTolerance = tolerance;
    }

    private void ParseCustomCondition(ConditionalJumpEvent conditionEvent)
    {
        Consume(TokenType.KeywordCustom);
        Expect(TokenType.ParenOpen, "'Custom' 后面需要 '('");
        // Lexer 已经将 `...` 内的内容提取为 Identifier Token
        var expressionToken = Expect(TokenType.Identifier, "需要 Custom 条件表达式字符串 (用反引号 `` 包裹)");
        Expect(TokenType.ParenClose, "Custom 表达式后需要 ')'");

        conditionEvent.ConditionType = ConditionType.CustomExpression;
        conditionEvent.CustomCondition = expressionToken.Value;
    }

    // --- 辅助方法 ---

    private Token CurrentToken() => _tokens[_currentTokenIndex];
    private Token PreviousToken() => _tokens[_currentTokenIndex - 1];

    private bool IsAtEnd() => _currentTokenIndex >= _tokens.Count || CurrentToken().Type == TokenType.EndOfFile;

    private Token Consume()
    {
        if (!IsAtEnd()) _currentTokenIndex++;
        return PreviousToken();
    }

    private Token Consume(TokenType expectedType, string? errorMessage = null)
    {
        var token = CurrentToken();
        if (token.Type == expectedType)
        {
            return Consume();
        }
        throw CreateException(errorMessage ?? $"期望 Token 类型 '{expectedType}'，但得到 '{token.Type}' ('{token.Value}')");
    }

    private Token Expect(TokenType expectedType, string errorMessage)
    {
        var token = CurrentToken();
        if (token.Type != expectedType)
        {
            throw CreateException(errorMessage + $". 得到 '{token.Type}' ('{token.Value}')");
        }
        return Consume(); // 消耗并返回预期的 Token
    }

    private void ExpectEndOfStatement(string contextMessage)
    {
        var token = CurrentToken();
        if (token.Type != TokenType.EndOfLine && token.Type != TokenType.EndOfFile)
        {
            throw CreateException($"{contextMessage}需要换行符或文件结束，但得到 '{token.Value}' ({token.Type})");
        }
        // 不消耗 EndOfLine 或 EndOfFile，留给 ParseStatement 循环处理
    }

    private string NewLabel(string prefix = "auto") => $"__dsl_{prefix}_{_labelCounter++}";

    // 添加用于跳转目标的 Nop 事件，并记录标签定义
    private void AddLabelEvent(string labelName)
    {
        if (!_labelsDefined.ContainsKey(labelName)) // 防止重复添加内部标签
        {
            _labelsDefined.Add(labelName, true);
            _events.Add(new Nop(labelName));
        }
        else if (!labelName.StartsWith("__dsl_")) // 如果是用户定义的标签重复，则报错
        {
            // 注意：这里可能需要更精确的行号，但这需要 Lexer/Parser 更紧密地协作
            // 或在 ResolveJumps 阶段再次检查
            // 为了简化，我们暂时允许内部标签逻辑“覆盖”用户标签定义点
            // 但在 ResolveJumps 会检查用户标签是否明确定义
            System.Diagnostics.Debug.WriteLine($"警告: 内部标签 '{labelName}' 可能覆盖了用户定义的标签");
        }
    }

    private TokenType CurrentBlockType()
    {
        if (_blockStack.Count == 0) return TokenType.Unknown; // 或者抛出异常
        return _blockStack.Peek().Type;
    }

    private void ResolveJumps()
    {
        foreach (var (jumpEvent, targetName, lineNumber) in _gotoFixups)
        {
            if (!_labelsDefined.ContainsKey(targetName))
            {
                throw CreateException($"未找到 'Goto' 语句的目标标签 '{targetName}'", lineNumber);
            }
            jumpEvent.TargetEventName = targetName; // 设置跳转目标
        }

        // 注意：ConditionalJump 的目标已经在解析时设置了名称，这里可以添加验证
        foreach (var (condEvent, trueTarget, falseTarget, line) in _conditionalJumpFixups)
        {
            if (!string.IsNullOrEmpty(trueTarget) && !_labelsDefined.ContainsKey(trueTarget))
            {
                throw CreateException($"未找到条件跳转的 True 目标标签 '{trueTarget}'", line);
            }
            if (!string.IsNullOrEmpty(falseTarget) && !_labelsDefined.ContainsKey(falseTarget))
            {
                throw CreateException($"未找到条件跳转的 False 目标标签 '{falseTarget}'", line);
            }
            // 实际的目标名称已经在解析 IF/WHILE 时设置好了
        }
    }

    private DslParserException CreateException(string message, int? lineNumber = null)
    {
        // 尝试获取当前 Token 的行号，如果 lineNumber 未提供
        int line = lineNumber ?? CurrentToken()?.LineNumber ?? _tokens.LastOrDefault()?.LineNumber ?? 0;
        return new DslParserException(message, line);
    }
}
