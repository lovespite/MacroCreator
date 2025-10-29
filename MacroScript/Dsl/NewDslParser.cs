using MacroCreator.Models;
using MacroCreator.Models.Events;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
        while (!IsAtEnd())
        {
            ParseStatement();
            // Expect EndOfLine or EndOfFile after a statement
            if (!IsAtEnd() && CurrentToken().Type != TokenType.EndOfLine)
            {
                throw CreateException($"Expected end of line or end of file after statement, but got {CurrentToken().Type}");
            }
            // Consume the EndOfLine token after a statement (if present)
            if (!IsAtEnd() && CurrentToken().Type == TokenType.EndOfLine)
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
    private Token CurrentToken() => _currentToken;

    /// <summary>
    /// Checks if the parser has reached the end of the token stream.
    /// </summary>
    private bool IsAtEnd() => CurrentToken().Type == TokenType.EndOfFile;

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
        var token = CurrentToken();
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
        var token = CurrentToken();
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
            case TokenType.KeywordScript: ParseScriptStatement(); break;
            // EndOfFile is handled by the main loop
            case TokenType.EndOfFile: break;
            // Ignore leading EOL tokens (should be rare after filtering)
            case TokenType.EndOfLine: Consume(); break; // Skip and next iteration will ParseStatement
            default:
                throw CreateException($"Unexpected token '{token.Value}' ({token.Type}), expected start of a statement");
        }
    }

    // --- Specific Statement Parsers ---

    private void ParseIfStatement()
    {
        int line = CurrentToken().LineNumber;
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

    private void ParseElseStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordElse);

        if (_blockStack.Count == 0 || CurrentBlockType() != TokenType.KeywordIf)
            throw CreateException($"Unexpected 'else' without a matching 'if'");

        var ifBlock = _blockStack.Pop();

        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, ifBlock.EndLabel, line));
        _events.Add(jumpToEnd);

        if (string.IsNullOrEmpty(ifBlock.ElseLabel)) throw CreateException($"Internal error: If block missing else label", line);
        AddLabelEvent(ifBlock.ElseLabel);

        _blockStack.Push(new FlowControlBlock(TokenType.KeywordElse, null, null, ifBlock.EndLabel, line));
    }

    private void ParseEndIfStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordEndIf);

        if (_blockStack.Count == 0 || (CurrentBlockType() != TokenType.KeywordIf && CurrentBlockType() != TokenType.KeywordElse))
            throw CreateException($"Unexpected 'endif' without a matching 'if' or 'else'");

        var block = _blockStack.Pop();

        if (block.Type == TokenType.KeywordIf) // If block without an else
        {
            if (string.IsNullOrEmpty(block.ElseLabel)) throw CreateException($"Internal error: If block missing else label", line);
            AddLabelEvent(block.ElseLabel); // Add the target for the false condition jump
        }

        AddLabelEvent(block.EndLabel); // Add the final exit label
    }

    private void ParseWhileStatement()
    {
        int line = CurrentToken().LineNumber;
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
        int line = CurrentToken().LineNumber;
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

    private void ParseBreakStatement()
    {
        int line = CurrentToken().LineNumber;
        Consume(TokenType.KeywordBreak);

        var whileBlock = _blockStack.FirstOrDefault(b => b.Type == TokenType.KeywordWhile);
        if (whileBlock == null)
            throw CreateException($"'break' can only be used inside a 'while' loop");

        var jumpToEnd = new JumpEvent { TimeSinceLastEvent = 0 };
        _gotoFixups.Add((jumpToEnd, whileBlock.EndLabel, line));
        _events.Add(jumpToEnd);
    }

    private void ParseLabelStatement()
    {
        int line = CurrentToken().LineNumber;
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
        int line = CurrentToken().LineNumber;
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

    private void ParseDelayStatement()
    {
        Consume(TokenType.KeywordDelay);
        Expect(TokenType.ParenOpen, "'Delay' requires '(' after it");
        var numberToken = Expect(TokenType.Number, "Requires delay milliseconds");
        Expect(TokenType.ParenClose, "Milliseconds requires ')' after it");

        if (!double.TryParse(numberToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double delay))
            throw CreateException($"Invalid delay milliseconds '{numberToken.Value}'");

        _events.Add(new DelayEvent { DelayMilliseconds = (int)Math.Round(delay), TimeSinceLastEvent = 0 }); // Use Math.Round for double -> int
    }

    private void ParseMouseStatement()
    {
        Consume(TokenType.KeywordMouse);
        Expect(TokenType.ParenOpen, "'Mouse' requires '(' after it");

        var actionToken = Expect(TokenType.Identifier, "Requires MouseAction (e.g., LeftDown, Move)");
        if (!Enum.TryParse<MouseAction>(actionToken.Value, true, out var action))
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
            var delayToken = Expect(TokenType.Number, "Requires delay milliseconds for Mouse event");
            if (!double.TryParse(delayToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw CreateException($"Invalid delay milliseconds '{delayToken.Value}'");
        }

        Expect(TokenType.ParenClose, "Mouse parameter list requires ')' after it");

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
        if (!Enum.TryParse<KeyboardAction>(actionToken.Value, true, out var action))
            throw CreateException($"Unknown KeyboardAction '{actionToken.Value}'");

        Expect(TokenType.Comma, "KeyboardAction requires ',' after it");
        var keyToken = Expect(TokenType.Identifier, "Requires Keys enum value (e.g., LControlKey, A)");

        string keyValue = keyToken.Value;
        // Handle common problematic key names if needed (Lexer might handle some)
        if (keyValue.Equals("Comma", StringComparison.OrdinalIgnoreCase)) keyValue = "Oemcomma";
        // Add more mappings if Lexer doesn't handle them (e.g., Minus -> OemMinus)

        if (!Enum.TryParse<System.Windows.Forms.Keys>(keyValue, true, out var key))
        {
            throw CreateException($"Unknown Keys enum value '{keyToken.Value}'");
        }

        double delayMs = 0;
        // Optional delay parameter
        if (CurrentToken().Type == TokenType.Comma)
        {
            Consume(); // Consume ','
            var delayToken = Expect(TokenType.Number, "Requires delay milliseconds for Key event");
            if (!double.TryParse(delayToken.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out delayMs))
                throw CreateException($"Invalid delay milliseconds '{delayToken.Value}'");
        }

        Expect(TokenType.ParenClose, "Key parameter list requires ')' after it");

        _events.Add(new KeyboardEvent
        {
            Action = action,
            Key = key,
            TimeSinceLastEvent = delayMs
        });
    }

    // script(string content, string? name)
    private void ParseScriptStatement()
    {
        int startLine = CurrentToken().LineNumber;
        Consume(TokenType.KeywordScript);
        string? scriptName = null;
        Expect(TokenType.ParenOpen, "'script' requires '(' after it");

        var contentToken = Expect(TokenType.StringLiteral, "Requires script content string (in quotes or backticks)");

        var scriptContent = contentToken.Value.Trim(); // Raw content with quotes/backticks

        // Optional name parameter
        if (CurrentToken().Type == TokenType.Comma)
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
            isNotEquals = false; // Custom condition implies '== true' by default
        }
        else
        {
            throw CreateException($"Expected condition (PixelColor or Custom), but got '{CurrentToken().Value}'");
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
        if (operatorToken.Type == TokenType.OperatorEquals) isNotEquals = false;
        else if (operatorToken.Type == TokenType.OperatorNotEquals) isNotEquals = true;
        else throw CreateException($"PixelColor condition requires '==' or '!=' operator");

        Color color;
        byte tolerance = 0;
        bool isArgb = false;
        int a = 255; // Default alpha

        if (CurrentToken().Type == TokenType.KeywordRGB || CurrentToken().Type == TokenType.KeywordARGB)
        {
            isArgb = CurrentToken().Type == TokenType.KeywordARGB;
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
            if (CurrentToken().Type == TokenType.Comma)
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
        int line = lineNumber ?? CurrentToken()?.LineNumber ?? 0; // Default to 0 if currentToken is somehow null
        return new DslParserException(message, line);
    }
}