using MacroCreator.Models;
using MacroCreator.Models.Events;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MacroScript.Dsl;

// --- 语法分析 (Parser) ---
public class NewDslParser
{
    // private List<Token> _tokens = null!;
    // private int _currentTokenIndex;

    // Token 流的迭代器
    private IEnumerator<Token> _tokenEnumerator = null!;
    private Token _currentToken = null!;

    private int _labelCounter = 0;
    private readonly List<MacroEvent> _events = [];
    private readonly Stack<FlowControlBlock> _blockStack = new();
    private readonly Dictionary<string, bool> _labelsDefined = new(System.StringComparer.OrdinalIgnoreCase);
    private readonly List<(JumpEvent Event, string TargetName, int LineNumber)> _gotoFixups = [];

    private class FlowControlBlock
    {
        public TokenType Type { get; }
        public string? StartLabel { get; } // For while loops
        public string? ElseLabel { get; }  // For if's false branch or while start
        public string EndLabel { get; }   // Exit point of the block
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

        // 1. 获取过滤后的 Token 迭代器
        _tokenEnumerator = FilterMeaningfulTokens(tokens).GetEnumerator();

        // 2. "启动" (Prime) 迭代器: 必须先调用 MoveNext() 来加载第一个 Token
        if (!_tokenEnumerator.MoveNext())
        {
            // 如果流为空 (即使是 Lexer 错误导致没有 EOF)，
            // 我们也创建一个 EOF Token 来保证解析器可以正常启动和停止。
            _currentToken = new Token(TokenType.EndOfFile, string.Empty, 1, 1);
        }
        else
        {
            _currentToken = _tokenEnumerator.Current;
        }


        // 3. 主循环 (无变化, 但现在依赖于重构后的 IsAtEnd() 和 Consume())
        while (!IsAtEnd())
        {
            ParseStatement();
            // Expect EndOfLine or EndOfFile after a statement
            if (!IsAtEnd() && CurrentToken().Type != TokenType.EndOfLine)
            {
                throw CreateException($"Expected end of line or end of file after statement, but got {CurrentToken().Type}");
            }
            // Consume the EndOfLine token after a statement
            if (!IsAtEnd() && CurrentToken().Type == TokenType.EndOfLine)
            {
                Consume();
            }
        }

        // 4. 结束检查 (无变化)
        if (_blockStack.Count > 0)
        {
            var block = _blockStack.Peek();
            throw CreateException($"Found unclosed '{block.Type}' block defined on line {block.LineNumber} at end of file", block.LineNumber);
        }

        ResolveJumps();

        return _events;
    }

    /// <summary>
    /// (已重构) 使用 yield return 过滤 Token 流
    /// </summary>
    private static IEnumerable<Token> FilterMeaningfulTokens(IEnumerable<Token> rawTokens)
    {
        bool lastWasEndOfLine = true; // Treat start as if preceded by newline

        foreach (var token in rawTokens)
        {
            if (token.Type == TokenType.EndOfLine)
            {
                if (!lastWasEndOfLine) // Keep only the first EndOfLine after other tokens
                {
                    yield return token;
                    lastWasEndOfLine = true;
                }
                // Skip consecutive EndOfLines (blank lines)
            }
            else if (token.Type == TokenType.EndOfFile)
            {
                yield return token; // 始终保留 EndOfFile
                yield break; // 迭代结束
            }
            else
            {
                yield return token;
                lastWasEndOfLine = false;
            }
        }

        // 注意：我们假设 Lexer 总是会提供一个 EndOfFile Token。
        // 即使输入为空，Lexer 也会返回 EOF。
        // 因此，这个迭代器 *总是* 会产生至少一个 EOF Token。
        // `Parse` 方法中的启动逻辑也处理了流为空的极端情况。
    }

    private void ParseStatement()
    {
        var token = CurrentToken();

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
            case TokenType.KeywordScript: ParseScriptStatement(); break; // <-- Add Script parsing
                                                                         // EndOfFile is handled by the main loop
            case TokenType.EndOfFile: break;
            // Ignore leading EndOfLine tokens if any survived filtering
            case TokenType.EndOfLine: Consume(); ParseStatement(); break; // Skip and parse next
            default:
                throw CreateException($"Unexpected token '{token.Value}' ({token.Type}), expected start of a statement");
        }
    }

    // --- Statement Parsing Methods ---

    private void ParseIfStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordIf); // if
        Expect(TokenType.ParenOpen, "'if' requires '(' after it");
        var (conditionEvent, jumpTargets) = ParseCondition(); // (condition)
        Expect(TokenType.ParenClose, "Condition requires ')' after it");

        string trueLabel = NewLabel("if_true");
        string elseLabel = NewLabel("if_else");
        string endLabel = NewLabel("if_end");

        // Setup jump targets considering the operator (== or !=)
        conditionEvent.TrueTargetEventName = jumpTargets.IsNotEquals ? elseLabel : trueLabel;
        conditionEvent.FalseTargetEventName = jumpTargets.IsNotEquals ? trueLabel : elseLabel;
        _events.Add(conditionEvent);

        AddLabelEvent(trueLabel); // Mark the start of the 'true' block

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordIf, null, elseLabel, endLabel, line));
    }

    private void ParseElseStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordElse);

        if (_blockStack.Count == 0 || _blockStack.Peek().Type != TokenType.KeywordIf)
            throw CreateException($"Unexpected 'else' without a matching 'if'");

        var ifBlock = _blockStack.Pop();

        // 1. Add GOTO to jump over the else block from the end of the if block
        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, ifBlock.EndLabel, line)); // Resolve target later
        _events.Add(jumpToEnd);

        // 2. Add the else label (start of the else block)
        if (string.IsNullOrEmpty(ifBlock.ElseLabel)) throw CreateException($"Internal error: If block missing else label", line);
        AddLabelEvent(ifBlock.ElseLabel);

        // 3. Push an "else" block onto the stack
        _blockStack.Push(new FlowControlBlock(TokenType.KeywordElse, null, null, ifBlock.EndLabel, line));
    }

    private void ParseEndIfStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordEndIf);

        if (_blockStack.Count == 0 || (CurrentBlockType() != TokenType.KeywordIf && CurrentBlockType() != TokenType.KeywordElse))
            throw CreateException($"Unexpected 'endif' without a matching 'if' or 'else'");

        var block = _blockStack.Pop();

        // If it was an 'if' block (no else), we still need the else label as the jump target for false condition
        if (block.Type == TokenType.KeywordIf)
        {
            if (string.IsNullOrEmpty(block.ElseLabel)) throw CreateException($"Internal error: If block missing else label", line);
            AddLabelEvent(block.ElseLabel);
        }

        // Add the endif label (the exit point)
        AddLabelEvent(block.EndLabel);
    }

    private void ParseWhileStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordWhile);
        Expect(TokenType.ParenOpen, "'while' requires '(' after it");
        var (conditionEvent, jumpTargets) = ParseCondition();
        Expect(TokenType.ParenClose, "Condition requires ')' after it");

        string startLabel = NewLabel("while_start"); // Label before condition check
        string bodyLabel = NewLabel("while_body");   // Label for the loop body start
        string endLabel = NewLabel("while_end");     // Label after the loop

        AddLabelEvent(startLabel); // Mark the start (condition check)

        // Setup jump targets considering the operator (== or !=)
        conditionEvent.TrueTargetEventName = jumpTargets.IsNotEquals ? endLabel : bodyLabel;   // If true (or not false), jump to body or end
        conditionEvent.FalseTargetEventName = jumpTargets.IsNotEquals ? bodyLabel : endLabel; // If false (or not true), jump to end or body
        _events.Add(conditionEvent);

        AddLabelEvent(bodyLabel); // Mark the start of the loop body

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordWhile, startLabel, null, endLabel, line));
    }

    private void ParseEndWhileStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordEndWhile);

        if (_blockStack.Count == 0 || CurrentBlockType() != TokenType.KeywordWhile)
            throw CreateException($"Unexpected 'endwhile' without a matching 'while'");

        var block = _blockStack.Pop();
        if (string.IsNullOrEmpty(block.StartLabel)) throw CreateException($"Internal error: While block missing start label", line);

        // Add GOTO to jump back to the start (condition check)
        var jumpToStart = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToStart, block.StartLabel, line)); // Resolve target later
        _events.Add(jumpToStart);

        // Add the end label (exit point)
        AddLabelEvent(block.EndLabel);
    }

    private void ParseBreakStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordBreak);

        // Find the innermost 'while' block
        var whileBlock = _blockStack.FirstOrDefault(b => b.Type == TokenType.KeywordWhile);
        if (whileBlock == null)
            throw CreateException($"'break' can only be used inside a 'while' loop");

        // Add GOTO to jump to the end of that 'while' loop
        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, whileBlock.EndLabel, line)); // Resolve target later
        _events.Add(jumpToEnd);
    }

    private void ParseLabelStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordLabel);
        // Expect(TokenType.ParenOpen, "'label' requires '(' after it");
        var labelToken = Expect(TokenType.Identifier, "Requires a label name");
        // Expect(TokenType.ParenClose, "Label name requires ')' after it");

        string labelName = labelToken.Value;
        if (labelName.StartsWith("__dsl_"))
            throw CreateException($"Invalid label name '{labelName}'. Cannot use the '__dsl_' prefix.", line);
        if (_labelsDefined.ContainsKey(labelName))
            throw CreateException($"Label '{labelName}' is already defined.", line);

        _labelsDefined.Add(labelName, true); // Mark as defined
        _events.Add(new Nop(labelName));     // Add the Nop event representing the label
    }

    private void ParseGotoStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordGoto);
        // Expect(TokenType.ParenOpen, "'goto' requires '(' after it");
        var labelToken = Expect(TokenType.Identifier, "Requires a target label name");
        // Expect(TokenType.ParenClose, "Label name requires ')' after it");

        var jumpEvent = new JumpEvent { TimeSinceLastEvent = 0 };
        // Add to fixup list, target name will be set during ResolveJumps
        _gotoFixups.Add((jumpEvent, labelToken.Value, line));
        _events.Add(jumpEvent);
    }

    private void ParseExitStatement()
    {
        Consume(TokenType.KeywordExit);
        _events.Add(new BreakEvent { TimeSinceLastEvent = 0 });
    }

    private void ParseDelayStatement()
    {
        Consume(TokenType.KeywordDelay);
        Expect(TokenType.ParenOpen, "'Delay' requires '(' after it");
        var numberToken = Expect(TokenType.Number, "Requires delay milliseconds");
        Expect(TokenType.ParenClose, "Milliseconds requires ')' after it");

        if (!double.TryParse(numberToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double delay))
            throw CreateException($"Invalid delay milliseconds '{numberToken.Value}'");

        _events.Add(new DelayEvent { DelayMilliseconds = (int)delay, TimeSinceLastEvent = 0 });
    }

    private void ParseMouseStatement()
    {
        Consume(TokenType.KeywordMouse);
        Expect(TokenType.ParenOpen, "'Mouse' requires '(' after it");

        var actionToken = Expect(TokenType.Identifier, "Requires MouseAction (e.g., LeftDown, Move)");
        if (!System.Enum.TryParse<MouseAction>(actionToken.Value, true, out var action))
            throw CreateException($"Unknown MouseAction '{actionToken.Value}'");

        Expect(TokenType.Comma, "MouseAction requires ',' after it");
        var xToken = Expect(TokenType.Number, "Requires X coordinate");
        if (!int.TryParse(xToken.Value, out int x)) throw CreateException($"Invalid X coordinate '{xToken.Value}'");

        Expect(TokenType.Comma, "X coordinate requires ',' after it");
        var yToken = Expect(TokenType.Number, "Requires Y coordinate");
        if (!int.TryParse(yToken.Value, out int y)) throw CreateException($"Invalid Y coordinate '{yToken.Value}'");

        int wheelDelta = 0;
        double delayMs = 0;

        if (action == MouseAction.Wheel)
        {
            Expect(TokenType.Comma, "Y coordinate requires ',' after it (for Wheel)");
            var deltaToken = Expect(TokenType.Number, "Requires WheelDelta");
            if (!int.TryParse(deltaToken.Value, out wheelDelta)) throw CreateException($"Invalid WheelDelta '{deltaToken.Value}'");
        }

        // Optional delay parameter
        if (CurrentToken().Type == TokenType.Comma)
        {
            Consume(); // Consume ','
            var delayToken = Expect(TokenType.Number, "Requires delay milliseconds");
            if (!double.TryParse(delayToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw CreateException($"Invalid delay milliseconds '{delayToken.Value}'");
        }

        Expect(TokenType.ParenClose, "Parameter list requires ')' after it");

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
        Consume(TokenType.KeywordKey);
        Expect(TokenType.ParenOpen, "'Key' requires '(' after it");

        var actionToken = Expect(TokenType.Identifier, "Requires KeyboardAction (e.g., KeyDown, KeyUp)");
        if (!System.Enum.TryParse<KeyboardAction>(actionToken.Value, true, out var action))
            throw CreateException($"Unknown KeyboardAction '{actionToken.Value}'");

        Expect(TokenType.Comma, "KeyboardAction requires ',' after it");
        var keyToken = Expect(TokenType.Identifier, "Requires Keys enum value (e.g., LControlKey, A)");
        // Special case for comma key name itself
        string keyValue = keyToken.Value;
        if (keyValue.Equals("Comma", StringComparison.OrdinalIgnoreCase))
        {
            keyValue = "Oemcomma"; // Map to the correct Keys enum member name
        }

        if (!System.Enum.TryParse<System.Windows.Forms.Keys>(keyValue, true, out var key))
        {
            // Try parsing potentially problematic names like ',' directly if needed,
            // but preferring explicit names like Oemcomma is better.
            if (keyToken.Value == "," && System.Enum.TryParse<System.Windows.Forms.Keys>("Oemcomma", true, out key))
            {
                // Parsed successfully as Oemcomma
            }
            else
            {
                throw CreateException($"Unknown Keys enum value '{keyToken.Value}'");
            }
        }


        double delayMs = 0;
        // Optional delay parameter
        if (CurrentToken().Type == TokenType.Comma)
        {
            Consume(); // Consume ','
            var delayToken = Expect(TokenType.Number, "Requires delay milliseconds");
            if (!double.TryParse(delayToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw CreateException($"Invalid delay milliseconds '{delayToken.Value}'");
        }

        Expect(TokenType.ParenClose, "Parameter list requires ')' after it");

        _events.Add(new KeyboardEvent
        {
            Action = action,
            Key = key,
            TimeSinceLastEvent = delayMs
        });
    }

    // --- Script Block Parsing ---
    private void ParseScriptStatement()
    {
        int startLine = CurrentToken().LineNumber;
        Consume(TokenType.KeywordScript);
        string? scriptName = null;
        if (CurrentToken().Type == TokenType.Identifier)
        {
            scriptName = Consume().Value; // Optional script name
        }
        Expect(TokenType.EndOfLine, "Need newline after 'script [name]'");

        var scriptLines = new List<string>();
        // var transformedLines = new List<string>();

        // Read lines until endscript
        while (!IsAtEnd() && CurrentToken().Type != TokenType.KeywordEndScript)
        {
            int currentLineNum = CurrentToken().LineNumber;
            StringBuilder lineBuilder = new();
            // Reconstruct the original line from tokens until EndOfLine
            while (!IsAtEnd() && CurrentToken().Type != TokenType.EndOfLine && CurrentToken().Type != TokenType.KeywordEndScript)
            {
                // Append raw token value (handle spacing appropriately if needed, but often not necessary for execution)
                lineBuilder.Append(CurrentToken().Value);
                Consume();
            }
            string originalLine = lineBuilder.ToString().Trim();
            scriptLines.Add(originalLine); // Store original line if needed for debugging?

            //// Transform the line
            //if (!string.IsNullOrWhiteSpace(originalLine))
            //{
            //    try
            //    {
            //        transformedLines.Add(TransformScriptLine(originalLine, currentLineNum));
            //    }
            //    catch (Exception ex) when (ex is not DslParserException)
            //    {
            //        // Catch transformation errors
            //        throw CreateException($"Error transforming script line: '{originalLine}'. Reason: {ex.Message}", currentLineNum);
            //    }
            //}

            // Consume the EndOfLine token separating script lines
            if (!IsAtEnd() && CurrentToken().Type == TokenType.EndOfLine)
            {
                Consume();
            }
            else if (!IsAtEnd() && CurrentToken().Type != TokenType.KeywordEndScript)
            {
                // Should not happen if lexer includes EndOfLine correctly
                throw CreateException("Expected newline within script block", CurrentToken().LineNumber);
            }
        }

        Expect(TokenType.KeywordEndScript, "Script block must end with 'endscript'");
        // Optional: Expect EndOfLine after endscript, consistent with other blocks
        // ExpectEndOfStatement("'endscript' keyword requires end of line or file");


        _events.Add(new ScriptEvent
        {
            EventName = scriptName, // Use the parsed script name
            ScriptLines = [..scriptLines],
            TimeSinceLastEvent = 0 // Script blocks execute instantly relative to the sequence
        });
    }


    // --- Condition Parsing ---
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
            isNotEquals = false; // Custom condition implies '== true'
        }
        else
        {
            throw CreateException($"Expected condition (PixelColor or Custom), but got '{CurrentToken().Value}'");
        }

        return (conditionEvent, new JumpTargets(isNotEquals));
    }

    // Helper struct for ParseCondition return value
    private record struct JumpTargets(bool IsNotEquals);


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
        if (operatorToken.Type == TokenType.OperatorEquals) isNotEquals = false;
        else if (operatorToken.Type == TokenType.OperatorNotEquals) isNotEquals = true;
        else throw CreateException($"PixelColor condition requires '==' or '!=' operator");

        Color color;
        byte tolerance = 0;
        if (CurrentToken().Type == TokenType.KeywordRGB)
        {
            Consume(); // RGB
            Expect(TokenType.ParenOpen, "'RGB' requires '(' after it");
            var rToken = Expect(TokenType.Number, "Requires R value");
            Expect(TokenType.Comma, "R value requires ',' after it");
            var gToken = Expect(TokenType.Number, "Requires G value");
            Expect(TokenType.Comma, "G value requires ',' after it");
            var bToken = Expect(TokenType.Number, "Requires B value");

            // Optional tolerance
            if (CurrentToken().Type == TokenType.Comma)
            {
                Consume(); // Consume ','
                var toleranceToken = Expect(TokenType.Number, "Requires tolerance value (0-255)");
                if (!byte.TryParse(toleranceToken.Value, out tolerance)) throw CreateException($"Invalid tolerance value '{toleranceToken.Value}'");
            }

            Expect(TokenType.ParenClose, "RGB parameters require ')' after it");

            if (!int.TryParse(rToken.Value, out int r) || r < 0 || r > 255 ||
                !int.TryParse(gToken.Value, out int g) || g < 0 || g > 255 ||
                !int.TryParse(bToken.Value, out int b) || b < 0 || b > 255)
                throw CreateException("Invalid RGB color value(s) (must be 0-255)");
            color = System.Drawing.Color.FromArgb(r, g, b);

        }
        // else if (CurrentToken().Type == TokenType.KeywordARGB) ... // Handle ARGB similarly if needed
        else
        {
            throw CreateException($"Expected 'RGB(...)' after PixelColor operator");
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
        Expect(TokenType.ParenOpen, "'Custom' requires '(' after it");
        // Lexer extracts content between backticks as an Identifier token
        var expressionToken = Expect(TokenType.Identifier, "Requires Custom condition expression string (in backticks ` `)");
        Expect(TokenType.ParenClose, "Custom expression requires ')' after it");

        conditionEvent.ConditionType = ConditionType.CustomExpression;
        conditionEvent.CustomCondition = expressionToken.Value;
    }

    // --- Script Transformation ---

    // Basic transformation for simple assignments and variable reads
    private static string TransformScriptLine(string line, int lineNumber)
    {
        line = line.Trim();
        if (string.IsNullOrEmpty(line)) return string.Empty;

        // Match assignment: $variable = expression
        var assignmentMatch = Regexes.Regex_VarAssignmentMatcher().Match(line);
        if (assignmentMatch.Success)
        {
            string varName = assignmentMatch.Groups["var"].Value;
            string expression = assignmentMatch.Groups["expr"].Value.Trim();
            string transformedExpression = TransformScriptExpression(expression, lineNumber);
            return $"set(\"{varName}\", {transformedExpression})";
        }
        else
        {
            // If not an assignment, assume it's an expression to be evaluated (potentially with side effects)
            // Transform any variable reads within it
            return TransformScriptExpression(line, lineNumber);
        }
    }

    // Simple recursive expression transformer (handles basic arithmetic and variable reads)
    // This needs to be significantly more robust for complex expressions.
    private static string TransformScriptExpression(string expression, int _)
    {
        // Replace $variable with (type)get("variable")
        // Need a simple way to infer type or make assumptions. Let's assume int for arithmetic.
        // This regex is basic and might have issues with nested structures or strings.
        string transformed = Regexes.Regex_VarReplacer().Replace(expression,
            match =>
            {
                string varName = match.Groups["var"].Value;
                //// Determine context - crude check for arithmetic operators nearby
                //// A proper parser is needed for robust type inference.
                //bool seemsArithmetic = expression.Contains('+') || expression.Contains('-') || expression.Contains('*') || expression.Contains('/');
                //string cast = seemsArithmetic ? "(int)" : "(object)"; // Assume int for arithmetic, else object
                return $"get(\"{varName}\")";
            }
        );

        // Potentially add more transformations here for function calls etc. if needed

        return transformed;
    }

    // --- Helper Methods ---

    private Token CurrentToken()
    {
        // _currentToken 总是由 Parse() 或 Consume() 保证已设置
        return _currentToken;
    }

    //private Token PreviousToken()
    //{
    //    if (_currentTokenIndex <= 0) return new Token(TokenType.Unknown, "", 0, 0); // Or handle error
    //    return _tokens[_currentTokenIndex - 1];
    //}

    private bool IsAtEnd() => CurrentToken().Type == TokenType.EndOfFile;

    private Token Consume()
    {
        var consumedToken = _currentToken;

        // 只有在 *不* 是 EOF 时才推进迭代器
        if (consumedToken.Type != TokenType.EndOfFile)
        {
            if (_tokenEnumerator.MoveNext())
            {
                _currentToken = _tokenEnumerator.Current;
            }
            else
            {
                // 如果 MoveNext 失败 (流意外结束，没有 EOF)
                // 我们制造一个 EOF 来安全地停止解析器。
                // 我们使用前一个 Token 的位置信息来估算新位置。
                _currentToken = new Token(TokenType.EndOfFile, string.Empty,
                    consumedToken.LineNumber,
                    consumedToken.Column + (consumedToken.Value?.Length ?? 0));
            }
        }

        // 返回刚刚被消耗的 Token
        return consumedToken;
    }

    // Consumes the current token if it matches the expected type, otherwise throws.
    private Token Consume(TokenType expectedType, string? errorMessage = null)
    {
        var token = CurrentToken();
        if (token.Type == expectedType)
        {
            return Consume();
        }
        throw CreateException(errorMessage ?? $"Expected token type '{expectedType}' but got '{token.Type}' ('{token.Value}')");
    }

    // Checks if the current token matches the expected type, consumes it, and returns it. Throws otherwise.
    private Token Expect(TokenType expectedType, string errorMessage)
    {
        var token = CurrentToken();
        if (token.Type != expectedType)
        {
            throw CreateException(errorMessage + $". Got '{token.Type}' ('{token.Value}') instead.");
        }
        return Consume(); // Consume and return the expected token
    }

    // Checks that the current token signifies the end of a statement (EndOfLine or EndOfFile)
    // Does NOT consume the token.
    private void ExpectEndOfStatement(string contextMessage)
    {
        if (IsAtEnd()) return; // EndOfFile is a valid end of statement

        var token = CurrentToken();
        if (token.Type != TokenType.EndOfLine)
        {
            throw CreateException($"{contextMessage} requires a newline or end of file, but got '{token.Value}' ({token.Type})");
        }
        // Do not consume EndOfLine here, let the main loop handle it.
    }

    private string NewLabel(string prefix = "auto") => $"__dsl_{prefix}_{_labelCounter++}";

    // Adds a Nop event for a label and marks it as defined
    private void AddLabelEvent(string labelName)
    {
        if (!_labelsDefined.ContainsKey(labelName)) // Avoid duplicate internal labels
        {
            _labelsDefined.Add(labelName, true);
            _events.Add(new Nop(labelName));
        }
        // Allow internal labels to potentially "reuse" a definition point
        // The final check happens in ResolveJumps for user-defined labels
    }

    private TokenType CurrentBlockType()
    {
        if (_blockStack.Count == 0) return TokenType.Unknown; // Or throw
        return _blockStack.Peek().Type;
    }

    // Final pass to resolve all goto targets
    private void ResolveJumps()
    {
        foreach (var (jumpEvent, targetName, lineNumber) in _gotoFixups)
        {
            if (!_labelsDefined.ContainsKey(targetName))
            {
                throw CreateException($"Target label '{targetName}' for 'Goto' on line {lineNumber} is not defined", lineNumber);
            }
            jumpEvent.TargetEventName = targetName; // Set the resolved target name
        }

        // Could add validation for conditional jumps here if needed, but targets should be set during parsing
    }

    private DslParserException CreateException(string message, int? lineNumber = null)
    {
        // 使用提供的行号，或者从当前 Token 获取行号
        // 因为 CurrentToken() 总是返回一个有效 Token (即使是 EOF)，所以这总是安全的。
        int line = lineNumber ?? CurrentToken().LineNumber;
        return new DslParserException(message, line);
    }
}


internal partial class Regexes
{
    [GeneratedRegex(@"^\s*\$(?<var>[a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(?<expr>.*)$", RegexOptions.Compiled)]
    public static partial Regex Regex_VarAssignmentMatcher();

    [GeneratedRegex(@"\$(?<var>[a-zA-Z_][a-zA-Z0-9_]*)")]
    public static partial Regex Regex_VarReplacer();
}