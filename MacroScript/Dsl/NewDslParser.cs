using MacroCreator.Models;
using MacroCreator.Models.Events;
using System.Globalization;

using Keys = MacroCreator.Models.Keys;
using Color = System.Drawing.Color;

namespace MacroScript.Dsl;

// --- 语法分析 (Parser) ---
public partial class NewDslParser
{
    // Token 流的迭代器
    private IEnumerator<Token> _tokenEnumerator = null!;
    private Token _currentToken = null!; // Holds the *next* token to be processed

    private int _labelCounter = 0;
    private readonly List<MacroEvent> _events = [];
    private readonly Stack<FlowControlBlock> _blockStack = new();
    private readonly Dictionary<string, bool> _labelsDefined = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(JumpEvent Event, string TargetName, int LineNumber)> _gotoFixups = [];

    // --- Types and Structures ---
    private class FlowControlBlock(TokenType type, string? startLabel, string? elseLabel, string endLabel, int lineNumber, string? varName = null, double step = 0)
    {
        public TokenType Type { get; } = type;
        public string? StartLabel { get; } = startLabel;
        public string? ElseLabel { get; } = elseLabel;
        public string EndLabel { get; } = endLabel;
        public int LineNumber { get; } = lineNumber;
        public string? VariableName { get; } = varName;
        public double StepValue { get; } = step;
    }

    // Helper struct for ParseCondition return value
    private record struct JumpTargets(bool IsNotEquals);

    // --- Public Parse Method ---
    /// <summary>
    /// 解析 Token 流
    /// </summary>
    public List<MacroEvent> Parse(IEnumerable<Token> tokens)
    {
        _events.Clear();
        _labelCounter = 0;
        _blockStack.Clear();
        _labelsDefined.Clear();
        _gotoFixups.Clear();

        _tokenEnumerator = FilterMeaningfulTokens(tokens).GetEnumerator();

        // Prime the iterator to load the first token
        MoveNextToken(); // Loads the initial _currentToken

        // Main parsing loop
        while (!EndOfFile)
        {
            ParseStatement();
            // Expect EndOfLine or EndOfFile after a statement
            if (!EndOfFile && CurrentToken.Type != TokenType.EndOfLine)
            {
                throw CreateException($"Expected end of line or end of file after statement, but got {CurrentToken.Type}");
            }
            // Consume the EndOfLine token after a statement (if present)
            if (!EndOfFile && CurrentToken.Type == TokenType.EndOfLine)
            {
                Consume(); // Consume EOL
            }
        }

        // Final checks
        if (_blockStack.Count > 0)
        {
            var block = _blockStack.Peek();
            throw CreateException($"Found unclosed '{block.Type}' block defined on line {block.LineNumber} at end of file", block.LineNumber);
        }

        ResolveJumps();

        return _events;
    }

    // --- Token Stream Handling ---

    /// <summary>
    /// Filters out whitespace and comments, manages consecutive EOLs.
    /// </summary>
    private static IEnumerable<Token> FilterMeaningfulTokens(IEnumerable<Token> rawTokens)
    {
        bool lastWasEndOfLine = true; // Treat start as if preceded by newline

        foreach (var token in rawTokens)
        {
            // Skip comments and whitespace (spaces/tabs)
            if (token.Type == TokenType.Comment || token.Type == TokenType.Whitespace)
            {
                continue;
            }

            if (token.Type == TokenType.EndOfLine)
            {
                if (!lastWasEndOfLine) // Keep only the first EOL after other tokens
                {
                    yield return token;
                    lastWasEndOfLine = true;
                }
                // Skip consecutive EOLs (blank lines)
            }
            else if (token.Type == TokenType.EndOfFile)
            {
                yield return token; // Always keep EOF
                yield break;       // Stop iteration
            }
            else // Any other meaningful token
            {
                yield return token;
                lastWasEndOfLine = false;
            }
        }
    }

    /// <summary>
    /// Advances the iterator and updates _currentToken. Handles end of stream.
    /// </summary>
    private void MoveNextToken()
    {
        if (_tokenEnumerator.MoveNext())
        {
            _currentToken = _tokenEnumerator.Current;
        }
        else
        {
            // Ensure _currentToken is EOF if iterator ends unexpectedly
            // Use position of the *last valid* token to estimate EOF position
            int lastLine = _currentToken?.LineNumber ?? 1;
            int lastCol = _currentToken != null ? (_currentToken.Column + (_currentToken.Value?.Length ?? 0)) : 1;
            _currentToken = new Token(TokenType.EndOfFile, string.Empty, lastLine, lastCol);
        }
    }

    /// <summary>
    /// Gets the current token (the one about to be processed).
    /// </summary>
    private Token CurrentToken => _currentToken;

    /// <summary>
    /// Checks if the parser has reached the end of the token stream.
    /// </summary>
    private bool EndOfFile => CurrentToken.Type == TokenType.EndOfFile;

    /// <summary>
    /// Consumes the current token and advances to the next one. Returns the consumed token.
    /// </summary>
    private Token Consume()
    {
        var consumedToken = _currentToken;
        if (consumedToken.Type != TokenType.EndOfFile)
        {
            MoveNextToken(); // Load the next token
        }
        return consumedToken;
    }

    /// <summary>
    /// Consumes the current token *if* it matches the expected type, otherwise throws.
    /// Returns the consumed token.
    /// </summary>
    private Token Consume(TokenType expectedType, string? errorMessage = null)
    {
        var token = CurrentToken;
        if (token.Type == expectedType)
        {
            return Consume();
        }
        throw CreateException(errorMessage ?? $"Expected token type '{expectedType}' but got '{token.Type}' ('{token.Value}')");
    }

    /// <summary>
    /// Checks if the current token matches the expected type, consumes it, and returns it. Throws otherwise.
    /// </summary>
    private Token Expect(TokenType expectedType, string errorMessage)
    {
        var token = CurrentToken;
        if (token.Type != expectedType)
        {
            throw CreateException(errorMessage + $". Got '{token.Type}' ('{token.Value}') instead.");
        }
        return Consume(); // Consume and return the expected token
    }

    // --- Statement Parsing ---

    /// <summary>
    /// Parses the next logical statement based on the current token.
    /// </summary>
    private void ParseStatement()
    {
        var token = CurrentToken;

        switch (token.Type)
        {
            case TokenType.KeywordIf: ParseIfStatement(); break;
            case TokenType.KeywordElseIf: ParseElseIfStatement(); break; // 新增
            case TokenType.KeywordElse: ParseElseStatement(); break;
            case TokenType.KeywordEndIf: ParseEndIfStatement(); break;
            case TokenType.KeywordWhile: ParseWhileStatement(); break;
            case TokenType.KeywordEndWhile: ParseEndWhileStatement(); break;
            case TokenType.KeywordFor: ParseForStatement(); break; // 新增
            case TokenType.KeywordEndFor: ParseEndForStatement(); break; // 新增
            case TokenType.KeywordBreak: ParseBreakStatement(); break;
            case TokenType.KeywordLabel: ParseLabelStatement(); break;
            case TokenType.KeywordGoto: ParseGotoStatement(); break;
            case TokenType.KeywordExit: ParseExitStatement(); break;
            case TokenType.KeywordScript: ParseScriptStatement(); break;
            case TokenType.Identifier: ParseFunctionCallStatement(); break;

            // EndOfFile is handled by the main loop
            case TokenType.EndOfFile: break;
            // Ignore leading EOL tokens (should be rare after filtering)
            case TokenType.EndOfLine: Consume(); break; // Skip and next iteration will ParseStatement
            default:
                throw CreateException($"Unexpected token '{token.Value}' ({token.Type}), expected start of a statement");
        }
    }

    #region Flow Control Statement Parsers

    // --- Specific Statement Parsers ---

    private void ParseIfStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordIf);
        Expect(TokenType.ParenOpen, "'if' requires '(' after it");
        var (conditionEvent, jumpTargets) = ParseCondition();
        Expect(TokenType.ParenClose, "Condition requires ')' after it");

        string trueLabel = NewLabel("if_true");
        string elseLabel = NewLabel("if_else");
        string endLabel = NewLabel("if_end");

        conditionEvent.TrueTargetEventName = jumpTargets.IsNotEquals ? elseLabel : trueLabel;
        conditionEvent.FalseTargetEventName = jumpTargets.IsNotEquals ? trueLabel : elseLabel;
        _events.Add(conditionEvent);

        AddLabelEvent(trueLabel); // Mark the start of the 'true' block

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordIf, null, elseLabel, endLabel, line));
    }

    private void ParseElseIfStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordElseIf);

        if (_blockStack.Count == 0 || (CurrentBlockType() != TokenType.KeywordIf && CurrentBlockType() != TokenType.KeywordElseIf))
            throw CreateException($"Unexpected 'elseif' without a matching 'if' or 'elseif'");

        var ifBlock = _blockStack.Pop();

        // 1. Add jump to the end of the *entire* if/elseif/else/endif chain
        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, ifBlock.EndLabel, line));
        _events.Add(jumpToEnd);

        // 2. Add the label for the *previous* block's false jump
        if (string.IsNullOrEmpty(ifBlock.ElseLabel)) throw CreateException($"Internal error: If/ElseIf block missing else label", line);
        AddLabelEvent(ifBlock.ElseLabel);

        // 3. Parse the new condition
        Expect(TokenType.ParenOpen, "'elseif' requires '(' after it");
        var (conditionEvent, jumpTargets) = ParseCondition();
        Expect(TokenType.ParenClose, "Condition requires ')' after it");

        // 4. Create new labels for this block
        string trueLabel = NewLabel("elseif_true");
        string newElseLabel = NewLabel("elseif_else");

        conditionEvent.TrueTargetEventName = jumpTargets.IsNotEquals ? newElseLabel : trueLabel;
        conditionEvent.FalseTargetEventName = jumpTargets.IsNotEquals ? trueLabel : newElseLabel;
        _events.Add(conditionEvent);

        // 5. Mark the start of the 'true' block
        AddLabelEvent(trueLabel);

        // 6. Push a new block for this 'elseif'
        _blockStack.Push(new FlowControlBlock(TokenType.KeywordElseIf, null, newElseLabel, ifBlock.EndLabel, line));
    }

    private void ParseElseStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordElse);

        if (_blockStack.Count == 0 || (CurrentBlockType() != TokenType.KeywordIf && CurrentBlockType() != TokenType.KeywordElseIf))
            throw CreateException($"Unexpected 'else' without a matching 'if' or 'elseif'");

        var ifBlock = _blockStack.Pop();

        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, ifBlock.EndLabel, line));
        _events.Add(jumpToEnd);

        if (string.IsNullOrEmpty(ifBlock.ElseLabel)) throw CreateException($"Internal error: If/ElseIf block missing else label", line);
        AddLabelEvent(ifBlock.ElseLabel);

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordElse, null, null, ifBlock.EndLabel, line));
    }

    private void ParseEndIfStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordEndIf);

        if (_blockStack.Count == 0 || (CurrentBlockType() != TokenType.KeywordIf && CurrentBlockType() != TokenType.KeywordElse && CurrentBlockType() != TokenType.KeywordElseIf))
            throw CreateException($"Unexpected 'endif' without a matching 'if', 'elseif', or 'else'");

        var block = _blockStack.Pop();

        if (block.Type == TokenType.KeywordIf || block.Type == TokenType.KeywordElseIf) // If block without an else
        {
            if (string.IsNullOrEmpty(block.ElseLabel)) throw CreateException($"Internal error: If/ElseIf block missing else label", line);
            AddLabelEvent(block.ElseLabel); // Add the target for the false condition jump
        }

        AddLabelEvent(block.EndLabel); // Add the final exit label
    }

    private void ParseWhileStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordWhile);
        Expect(TokenType.ParenOpen, "'while' requires '(' after it");
        var (conditionEvent, jumpTargets) = ParseCondition();
        Expect(TokenType.ParenClose, "Condition requires ')' after it");

        string startLabel = NewLabel("while_start");
        string bodyLabel = NewLabel("while_body");
        string endLabel = NewLabel("while_end");

        AddLabelEvent(startLabel); // Mark the start (condition check)

        conditionEvent.TrueTargetEventName = jumpTargets.IsNotEquals ? endLabel : bodyLabel;
        conditionEvent.FalseTargetEventName = jumpTargets.IsNotEquals ? bodyLabel : endLabel;
        _events.Add(conditionEvent);

        AddLabelEvent(bodyLabel); // Mark the start of the loop body

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordWhile, startLabel, null, endLabel, line));
    }

    private void ParseEndWhileStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordEndWhile);

        if (_blockStack.Count == 0 || CurrentBlockType() != TokenType.KeywordWhile)
            throw CreateException($"Unexpected 'endwhile' without a matching 'while'");

        var block = _blockStack.Pop();
        if (string.IsNullOrEmpty(block.StartLabel)) throw CreateException($"Internal error: While block missing start label", line);

        var jumpToStart = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToStart, block.StartLabel, line));
        _events.Add(jumpToStart);

        AddLabelEvent(block.EndLabel); // Add the exit label
    }

    private void ParseForStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordFor);

        Token varToken = Consume(TokenType.Identifier, "'for' requires a variable name (Identifier)");
        Consume(TokenType.OperatorEquals, "'for' loop variable requires '=' assignment");
        Token startToken = Consume(TokenType.Number, "'for' loop requires a numeric start value");
        Consume(TokenType.KeywordTo, "'for' loop requires 'to'");
        Token endToken = Consume(TokenType.Number, "'for' loop requires a numeric end value");

        double stepVal = 1.0;
        if (CurrentToken.Type == TokenType.KeywordStep)
        {
            Consume(); // Consume 'step'
            Token stepToken = Consume(TokenType.Number, "'step' requires a numeric value");
            if (!double.TryParse(stepToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out stepVal) || stepVal == 0)
                throw CreateException($"Invalid 'step' value '{stepToken.Value}'. Must be a non-zero number.", line);
        }

        if (!double.TryParse(startToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double startVal))
            throw CreateException($"Invalid 'for' loop start value '{startToken.Value}'", line);
        if (!double.TryParse(endToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double endVal))
            throw CreateException($"Invalid 'for' loop end value '{endToken.Value}'", line);

        // 1. Initialization Script
        _events.Add(new ScriptEvent
        {
            ScriptLines = [$"set(\"{varToken.Value}\", {startVal.ToString(CultureInfo.InvariantCulture)})"],
            TimeSinceLastEvent = 0
        });

        // 2. Labels
        string startLabel = NewLabel("for_start");
        string bodyLabel = NewLabel("for_body");
        string endLabel = NewLabel("for_end");

        // 3. Condition Check (at startLabel)
        AddLabelEvent(startLabel);
        string condition = stepVal > 0
            ? $"{varToken.Value} <= {endVal.ToString(CultureInfo.InvariantCulture)}"
            : $"{varToken.Value} >= {endVal.ToString(CultureInfo.InvariantCulture)}";

        var condEvent = new ConditionalJumpEvent
        {
            ConditionType = ConditionType.CustomExpression,
            CustomCondition = condition,
            TrueTargetEventName = bodyLabel,
            FalseTargetEventName = endLabel,
            TimeSinceLastEvent = 0
        };
        _events.Add(condEvent);

        // 4. Body Label
        AddLabelEvent(bodyLabel);

        // 5. Push block to stack
        _blockStack.Push(new FlowControlBlock(TokenType.KeywordFor, startLabel, null, endLabel, line, varToken.Value, stepVal));
    }

    private void ParseEndForStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordEndFor);

        if (_blockStack.Count == 0 || CurrentBlockType() != TokenType.KeywordFor)
            throw CreateException($"Unexpected 'endfor' without a matching 'for'");

        var block = _blockStack.Pop();
        if (string.IsNullOrEmpty(block.StartLabel) || string.IsNullOrEmpty(block.VariableName))
            throw CreateException($"Internal error: 'for' block is missing metadata", line);

        // 1. Increment Script
        _events.Add(new ScriptEvent
        {
            ScriptLines = [$"set(\"{block.VariableName}\", {block.VariableName} + {block.StepValue.ToString(CultureInfo.InvariantCulture)})"],
            TimeSinceLastEvent = 0
        });

        // 2. Jump back to start (condition check)
        var jumpToStart = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToStart, block.StartLabel, line));
        _events.Add(jumpToStart);

        // 3. End Label
        AddLabelEvent(block.EndLabel);
    }


    private void ParseBreakStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordBreak);

        // Find the nearest enclosing loop (while or for)
        var loopBlock = _blockStack.FirstOrDefault(b => b.Type == TokenType.KeywordWhile || b.Type == TokenType.KeywordFor);
        if (loopBlock == null)
            throw CreateException($"'break' can only be used inside a 'while' or 'for' loop");

        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, loopBlock.EndLabel, line));
        _events.Add(jumpToEnd);
    }

    private void ParseLabelStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordLabel);
        // Arguments are no longer wrapped in parentheses for label/goto
        var labelToken = Expect(TokenType.Identifier, "'label' requires a label name after it");

        string labelName = labelToken.Value;
        if (labelName.StartsWith("__dsl_"))
            throw CreateException($"Invalid label name '{labelName}'. Cannot use the '__dsl_' prefix.", line);
        if (_labelsDefined.ContainsKey(labelName))
            throw CreateException($"Label '{labelName}' is already defined.", line);

        _labelsDefined.Add(labelName, true);
        _events.Add(new Nop(labelName));
    }

    private void ParseGotoStatement()
    {
        int line = CurrentToken.LineNumber;
        Consume(TokenType.KeywordGoto);
        // Arguments are no longer wrapped in parentheses for label/goto
        var labelToken = Expect(TokenType.Identifier, "'goto' requires a target label name after it");

        var jumpEvent = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpEvent, labelToken.Value, line));
        _events.Add(jumpEvent);
    }

    private void ParseExitStatement()
    {
        Consume(TokenType.KeywordExit);
        _events.Add(new BreakEvent { TimeSinceLastEvent = 0 });
    }

    #endregion

    #region Functional Statement Parsers

    // script(string content, string? name)
    private void ParseScriptStatement()
    {
        int startLine = CurrentToken.LineNumber;
        Consume(TokenType.KeywordScript);
        string? scriptName = null;
        Expect(TokenType.ParenOpen, "'script' requires '(' after it");

        var contentToken = Expect(TokenType.StringLiteral, "Requires script content string (in quotes or backticks)");

        var scriptContent = contentToken.Value.Trim(); // Raw content with quotes/backticks

        // Optional name parameter
        if (CurrentToken.Type == TokenType.Comma)
        {
            Consume(); // Consume ','
            var nameToken = Expect(TokenType.StringLiteral, "Requires script name string (in quotes or backticks)");
            scriptName = nameToken.Value.Trim('\"', '\'', '`'); // Remove quotes/backticks
        }
        Expect(TokenType.ParenClose, "Script parameters require ')' after them");
        // Don't expect EOL here, let the main loop handle it

        _events.Add(new ScriptEvent
        {
            EventName = scriptName,
            ScriptLines = [
                ..scriptContent
                    .ToString()
                    .Split('\n')
                    .Select(x => x.TrimEnd('\r'))
            ], // Store raw content, trim trailing newline before endscript
            TimeSinceLastEvent = 0
        });
    }

    #endregion

    #region Function Call Parsing (New)

    /// <summary>
    /// 解析标识符开头的语句作为函数调用
    /// e.g., Delay(100)
    /// </summary>
    private void ParseFunctionCallStatement()
    {
        Token funcNameToken = Consume(TokenType.Identifier);
        Expect(TokenType.ParenOpen, $"'{funcNameToken.Value}' function call requires '(' after it");

        List<Token> args = ParseArguments();

        Expect(TokenType.ParenClose, $"Function call '{funcNameToken.Value}' requires ')' after parameters");

        MacroEvent[] ev = CreateEventsFromFunctionCall(funcNameToken, args);
        _events.AddRange(ev);
    }

    /// <summary>
    /// 解析括号内的参数列表
    /// </summary>
    private List<Token> ParseArguments()
    {
        var args = new List<Token>();
        if (CurrentToken.Type == TokenType.ParenClose)
        {
            return args; // 空参数列表
        }

        while (true)
        {
            var token = CurrentToken;
            if (token.Type == TokenType.Identifier || token.Type == TokenType.Number || token.Type == TokenType.StringLiteral)
            {
                args.Add(Consume());
            }
            else
            {
                throw CreateException($"Unexpected token in argument list: '{token.Value}' ({token.Type}). Expected Identifier, Number, or String.");
            }

            if (CurrentToken.Type == TokenType.Comma)
            {
                Consume(); // Consume ','
            }
            else if (CurrentToken.Type == TokenType.ParenClose)
            {
                break; // End of argument list
            }
            else
            {
                throw CreateException($"Unexpected token '{CurrentToken.Value}' after argument. Expected ',' or ')'.");
            }
        }
        return args;
    }

    /// <summary>
    /// 根据函数名和参数创建 MacroEvent
    /// </summary>
    private MacroEvent[] CreateEventsFromFunctionCall(Token funcName, List<Token> args)
    {
        string name = funcName.Value.ToLowerInvariant();
        int line = funcName.LineNumber;

        try
        {
            return name switch
            {
                "delay" => CreateDelayEvent(args, line),
                "mousemove" => CreateMouseMoveEvent(args, line),
                "mousemoveto" => CreateMouseMoveToEvent(args, line),
                "mousedown" => CreateMouseDownEvent(args, line),
                "mouseup" => CreateMouseUpEvent(args, line),
                "mouseclick" => CreateMouseClickEvent(args, line),
                "mousewheel" => CreateMouseWheelEvent(args, line),
                "keydown" => CreateKeyEvent(KeyboardAction.KeyDown, args, line),
                "keyup" => CreateKeyEvent(KeyboardAction.KeyUp, args, line),
                "keypress" => CreateKeyEvent(KeyboardAction.KeyPress, args, line),
                _ => throw CreateException($"Unknown function name '{funcName.Value}'", line),
            };
        }
        catch (Exception ex) when (ex is not DslParserException)
        {
            throw CreateException($"Error parsing arguments for '{funcName.Value}': {ex.Message}", line);
        }
    }

    // --- Event Creation Helpers ---

    private int ParseOptionalDelay(List<Token> args, int startIndex)
    {
        if (args.Count > startIndex && args[startIndex].Type == TokenType.Number)
        {
            if (int.TryParse(args[startIndex].Value, out int delay))
            {
                args.RemoveAt(startIndex); // Consume the delay argument
                return delay;
            }
        }
        return 0; // 默认延迟
    }

    private List<Keys> ParseKeyArguments(List<Token> args, out int delayMs)
    {
        var keys = new List<Keys>();
        delayMs = 0;
        int line = args.Count > 0 ? args[0].LineNumber : CurrentToken.LineNumber;

        if (args.Count == 0)
        {
            throw CreateException("Key function requires at least one key argument", line);
        }

        // 最后一个参数可能是延迟
        if (args.Count > 1 && args[^1].Type == TokenType.Number)
        {
            if (!int.TryParse(args[^1].Value, out delayMs))
                throw CreateException($"Invalid delay milliseconds '{args[^1].Value}'", line);
            args.RemoveAt(args.Count - 1); // 移除延迟参数
        }

        // 剩余的都应该是按键
        foreach (var arg in args)
        {
            if (arg.Type != TokenType.Identifier)
                throw CreateException($"Expected key name (Identifier) but got '{arg.Value}' ({arg.Type})", arg.LineNumber);
            if (!Enum.TryParse<Keys>(arg.Value, true, out Keys key))
                throw CreateException($"Invalid key name '{arg.Value}'", arg.LineNumber);
            keys.Add(key);
        }

        return keys;
    }

    private MacroEvent[] CreateDelayEvent(List<Token> args, int line)
    {
        if (args.Count != 1 || args[0].Type != TokenType.Number)
            throw CreateException("'Delay' requires exactly one numeric argument (milliseconds)", line);

        if (!double.TryParse(args[0].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double delay))
            throw CreateException($"Invalid delay milliseconds '{args[0].Value}'", line);

        return [new DelayEvent { DelayMilliseconds = (int)Math.Round(delay), TimeSinceLastEvent = 0 }];
    }

    private MacroEvent[] CreateMouseMoveEvent(List<Token> args, int line)
    {
        int delayMs = ParseOptionalDelay(args, 2); // 检查第三个参数 (index 2)
        if (args.Count != 2 || args[0].Type != TokenType.Number || args[1].Type != TokenType.Number)
            throw CreateException("'MouseMove' requires two numeric arguments (X, Y) and an optional delay", line);

        if (!int.TryParse(args[0].Value, out int x)) throw CreateException($"Invalid X coordinate '{args[0].Value}'", line);
        if (!int.TryParse(args[1].Value, out int y)) throw CreateException($"Invalid Y coordinate '{args[1].Value}'", line);

        return [new MouseEvent { Action = MouseAction.Move, X = x, Y = y, TimeSinceLastEvent = delayMs }];
    }

    private MacroEvent[] CreateMouseMoveToEvent(List<Token> args, int line)
    {
        int delayMs = ParseOptionalDelay(args, 2); // 检查第三个参数 (index 2)
        if (args.Count != 2 || args[0].Type != TokenType.Number || args[1].Type != TokenType.Number)
            throw CreateException("'MouseMoveTo' requires two numeric arguments (X, Y) and an optional delay", line);

        if (!int.TryParse(args[0].Value, out int x)) throw CreateException($"Invalid X coordinate '{args[0].Value}'", line);
        if (!int.TryParse(args[1].Value, out int y)) throw CreateException($"Invalid Y coordinate '{args[1].Value}'", line);

        return [new MouseEvent { Action = MouseAction.MoveTo, X = x, Y = y, TimeSinceLastEvent = delayMs }];
    }

    private MouseAction ParseMouseButton(Token arg, int line)
    {
        if (arg.Type != TokenType.Identifier)
            throw CreateException("Expected mouse button (Left, Right, Middle)", line);

        return arg.Value.ToLowerInvariant() switch
        {
            "left" => MouseAction.LeftDown, // Base action, will be adjusted
            "right" => MouseAction.RightDown,
            "middle" => MouseAction.MiddleDown,
            _ => throw CreateException($"Invalid mouse button '{arg.Value}'. Expected 'Left', 'Right', or 'Middle'.", line),
        };
    }

    private MacroEvent[] CreateMouseDownEvent(List<Token> args, int line)
    {
        int delayMs = ParseOptionalDelay(args, 1);
        if (args.Count != 1) throw CreateException("'MouseDown' requires one button argument (Left, Right, Middle) and an optional delay", line);

        var action = ParseMouseButton(args[0], line); // Gets LeftDown, RightDown, or MiddleDown
        return [new MouseEvent { Action = action, TimeSinceLastEvent = delayMs }];
    }

    private MacroEvent[] CreateMouseUpEvent(List<Token> args, int line)
    {
        int delayMs = ParseOptionalDelay(args, 1);
        if (args.Count != 1) throw CreateException("'MouseUp' requires one button argument (Left, Right, Middle) and an optional delay", line);

        var action = ParseMouseButton(args[0], line); // Gets LeftDown, RightDown, or MiddleDown
        return [new MouseEvent { Action = action.GetPairedAction(), TimeSinceLastEvent = delayMs }]; // Converts to Up action
    }

    private MacroEvent[] CreateMouseClickEvent(List<Token> args, int line)
    {
        // MouseClick is special, it creates two events.
        // This parser structure only allows returning one.
        // We'll return the Down event and add the Up event directly.

        int delayMs = ParseOptionalDelay(args, 1);
        if (args.Count != 1) throw CreateException("'MouseClick' requires one button argument (Left, Right, Middle) and an optional delay", line);

        var downAction = ParseMouseButton(args[0], line);
        var upAction = downAction.GetPairedAction();

        // Add the Up event directly
        _events.Add(new MouseEvent { Action = upAction, TimeSinceLastEvent = delayMs });

        // Return the Down event
        return [new MouseEvent { Action = downAction, TimeSinceLastEvent = 0 }];
    }

    private MacroEvent[] CreateMouseWheelEvent(List<Token> args, int line)
    {
        int delayMs = ParseOptionalDelay(args, 1);
        if (args.Count != 1 || args[0].Type != TokenType.Number)
            throw CreateException("'MouseWheel' requires one numeric argument (delta) and an optional delay", line);

        if (!int.TryParse(args[0].Value, out int delta)) throw CreateException($"Invalid wheel delta '{args[0].Value}'", line);

        return [new MouseEvent { Action = MouseAction.Wheel, WheelDelta = delta, TimeSinceLastEvent = delayMs }];
    }

    private MacroEvent[] CreateKeyEvent(KeyboardAction action, List<Token> args, int line)
    {
        // Key functions are also special, they can take multiple keys and create multiple events.
        // We will add all but the first event directly, and return the first.

        List<Keys> keys = ParseKeyArguments(args, out int delayMs);

        if (keys.Count == 0)
            throw CreateException("Key function requires at least one key name argument", line);

        List<MacroEvent> keyEvents = [];

        foreach (var key in keys)
        {
            if (action == KeyboardAction.KeyPress)
            {
                keyEvents.Add(new KeyboardEvent { Action = KeyboardAction.KeyDown, Key = key, TimeSinceLastEvent = 0 });
                keyEvents.Add(new KeyboardEvent { Action = KeyboardAction.KeyUp, Key = key, TimeSinceLastEvent = 0 });
            }
            else
            {
                keyEvents.Add(new KeyboardEvent { Action = action, Key = key, TimeSinceLastEvent = 0 });
            }
        }

        // Apply delay to the *last* event in the sequence generated by this call
        if (keyEvents.Count > 0)
        {
            keyEvents[^1].TimeSinceLastEvent = delayMs;
        }

        // Return the first event
        return [.. keyEvents];
    }


    #endregion

    #region Condition Expression Parsing
    // --- Condition Parsing ---

    private (ConditionalJumpEvent conditionEvent, JumpTargets jumpTargets) ParseCondition()
    {
        var conditionEvent = new ConditionalJumpEvent { TimeSinceLastEvent = 0 };
        bool isNotEquals = false;

        if (CurrentToken.Type == TokenType.KeywordPixelColor)
        {
            ParsePixelColorCondition(conditionEvent, out isNotEquals);
        }
        else if (CurrentToken.Type == TokenType.KeywordCustom)
        {
            ParseCustomCondition(conditionEvent);
            isNotEquals = false; // Custom condition implies '== true' by default
        }
        else
        {
            throw CreateException($"Expected condition (PixelColor or Custom), but got '{CurrentToken.Value}'");
        }

        return (conditionEvent, new JumpTargets(isNotEquals));
    }


    private void ParsePixelColorCondition(ConditionalJumpEvent conditionEvent, out bool isNotEquals)
    {
        Consume(TokenType.KeywordPixelColor);
        Expect(TokenType.ParenOpen, "'PixelColor' requires '(' after it");
        var xToken = Expect(TokenType.Number, "Requires X coordinate");
        if (!int.TryParse(xToken.Value, out int x)) throw CreateException($"Invalid X coordinate '{xToken.Value}'");
        Expect(TokenType.Comma, "X coordinate requires ',' after it");
        var yToken = Expect(TokenType.Number, "Requires Y coordinate");
        if (!int.TryParse(yToken.Value, out int y)) throw CreateException($"Invalid Y coordinate '{yToken.Value}'");
        Expect(TokenType.ParenClose, "Y coordinate requires ')' after it");

        var operatorToken = Consume();
        if (operatorToken.Type == TokenType.OperatorCompareEquals) isNotEquals = false;
        else if (operatorToken.Type == TokenType.OperatorNotEquals) isNotEquals = true;
        else throw CreateException($"PixelColor condition requires '==' or '!=' operator");

        Color color;
        byte tolerance = 0;
        bool isArgb = false;
        int a = 255; // Default alpha

        if (CurrentToken.Type == TokenType.KeywordRGB || CurrentToken.Type == TokenType.KeywordARGB)
        {
            isArgb = CurrentToken.Type == TokenType.KeywordARGB;
            Consume(); // RGB or ARGB
            Expect(TokenType.ParenOpen, $"'{(isArgb ? "ARGB" : "RGB")}' requires '(' after it");

            if (isArgb)
            {
                var aToken = Expect(TokenType.Number, "Requires Alpha value");
                if (!int.TryParse(aToken.Value, out a) || a < 0 || a > 255)
                    throw CreateException("Invalid Alpha value (must be 0-255)");
                Expect(TokenType.Comma, "Alpha value requires ',' after it");
            }

            var rToken = Expect(TokenType.Number, "Requires R value");
            Expect(TokenType.Comma, "R value requires ',' after it");
            var gToken = Expect(TokenType.Number, "Requires G value");
            Expect(TokenType.Comma, "G value requires ',' after it");
            var bToken = Expect(TokenType.Number, "Requires B value");

            // Optional tolerance
            if (CurrentToken.Type == TokenType.Comma)
            {
                Consume(); // Consume ','
                var toleranceToken = Expect(TokenType.Number, "Requires tolerance value (0-255)");
                if (!byte.TryParse(toleranceToken.Value, out tolerance)) throw CreateException($"Invalid tolerance value '{toleranceToken.Value}'");
            }

            Expect(TokenType.ParenClose, $"{(isArgb ? "ARGB" : "RGB")} parameters require ')' after it");

            if (!int.TryParse(rToken.Value, out int r) || r < 0 || r > 255 ||
                !int.TryParse(gToken.Value, out int g) || g < 0 || g > 255 ||
                !int.TryParse(bToken.Value, out int b) || b < 0 || b > 255)
                throw CreateException("Invalid RGB color value(s) (must be 0-255)");
            color = Color.FromArgb(a, r, g, b); // Use potentially parsed 'a' value
        }
        else
        {
            throw CreateException($"Expected 'RGB(...)' or 'ARGB(...)' after PixelColor operator");
        }

        conditionEvent.ConditionType = ConditionType.PixelColor;
        conditionEvent.X = x;
        conditionEvent.Y = y;
        conditionEvent.ExpectedColor = color.ToArgb(); // Store as ARGB integer
        conditionEvent.PixelTolerance = tolerance;
    }

    private void ParseCustomCondition(ConditionalJumpEvent conditionEvent)
    {
        Consume(TokenType.KeywordCustom);
        Expect(TokenType.ParenOpen, "'Custom' requires '(' after it");
        // Expect a string literal (which includes backticks now)
        var expressionToken = Expect(TokenType.StringLiteral, "Requires Custom condition expression string (in quotes or backticks)");
        Expect(TokenType.ParenClose, "Custom expression requires ')' after it");

        conditionEvent.ConditionType = ConditionType.CustomExpression;
        // The value of StringLiteral token already has quotes removed and escapes processed by the lexer
        conditionEvent.CustomCondition = expressionToken.Value;
    }

    #endregion

    // --- Helper & Utility Methods ---

    private string NewLabel(string prefix = "auto") => $"__dsl_{prefix}_{_labelCounter++}";

    private void AddLabelEvent(string labelName)
    {
        if (!_labelsDefined.ContainsKey(labelName))
        {
            _labelsDefined.Add(labelName, true);
            _events.Add(new Nop(labelName));
        }
    }

    private TokenType CurrentBlockType()
    {
        return _blockStack.Count == 0 ? TokenType.Unknown : _blockStack.Peek().Type;
    }

    private void ResolveJumps()
    {
        foreach (var (jumpEvent, targetName, lineNumber) in _gotoFixups)
        {
            if (!_labelsDefined.ContainsKey(targetName))
            {
                throw CreateException($"Target label '{targetName}' for statement on line {lineNumber} is not defined", lineNumber);
            }
            jumpEvent.TargetEventName = targetName;
        }
    }

    private DslParserException CreateException(string message, int? lineNumber = null)
    {
        // Use provided line number or the current token's line number
        int line = lineNumber ?? CurrentToken?.LineNumber ?? 0; // Default to 0 if currentToken is somehow null
        return new DslParserException(message, line);
    }
}
