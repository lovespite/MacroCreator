using MacroCreator.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace MacroScript.Dsl;

/// <summary>
/// 词法分析器，将 DSL 脚本字符串转换为 Token 序列
/// </summary>
public class Lexer : IDisposable
{
    private readonly StreamReader _reader;
    private int _lineNumber;
    private int _column;

    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }

    // 关键字映射 (无变化)
    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "if", TokenType.KeywordIf },
        { "else", TokenType.KeywordElse },
        { "endif", TokenType.KeywordEndIf },
        { "while", TokenType.KeywordWhile },
        { "endwhile", TokenType.KeywordEndWhile },
        { "break", TokenType.KeywordBreak },
        { "label", TokenType.KeywordLabel },
        { "goto", TokenType.KeywordGoto },
        { "exit", TokenType.KeywordExit },
        { "delay", TokenType.KeywordDelay },
        { "mousedown", TokenType.KeywordMouseDown },
        { "mouseup", TokenType.KeywordMouseUp },
        { "mouseclick", TokenType.KeywordMouseClick },
        { "mousemove", TokenType.KeywordMouseMove },
        { "mousemoveto", TokenType.KeywordMouseMoveTo },
        { "mousewheel", TokenType.KeywordMouseWheel },
        { "keydown", TokenType.KeywordKeyDown },
        { "keyup", TokenType.KeywordKeyUp },
        { "keypress", TokenType.KeywordKeyPress },
        { "pixelcolor", TokenType.KeywordPixelColor },
        { "rgb", TokenType.KeywordRGB },
        { "argb", TokenType.KeywordARGB },
        { "custom", TokenType.KeywordCustom },
        { "script", TokenType.KeywordScript },
    };

    /// <summary>
    /// 构造函数，接收一个 StreamReader
    /// </summary>
    public Lexer(Stream stream, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _reader = new StreamReader(stream, encoding);
        _lineNumber = 1;
        _column = 1;
    }

    public Lexer(Stream stream)
        : this(stream, Encoding.UTF8)
    {
    }

    /// <summary>
    /// 构造函数，接收一个文件名
    /// </summary>
    /// <param name="filename"></param>
    public Lexer(string filename)
        : this(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
    {
    }

    /// <summary>
    /// 生成所有 Token
    /// </summary>
    public IEnumerable<Token> Tokenize()
    {
        // 循环直到流的末尾
        while (!_reader.EndOfStream)
        {
            var token = NextToken();

            // NextToken 可能会在流的末尾返回 EndOfFile，尽管循环条件已检查
            if (token.Type == TokenType.EndOfFile) break;

            // 过滤掉空白 (space/tab) 和注释。
            if (token.Type != TokenType.Whitespace && token.Type != TokenType.Comment)
                yield return token;
        }

        // 循环结束后, 返回最后的 EndOfFile 标记
        yield return new Token(TokenType.EndOfFile, string.Empty, _lineNumber, _column);
    }

    /// <summary>
    /// 获取下一个 Token
    /// </summary>
    private Token NextToken()
    {
        if (_reader.EndOfStream)
        {
            return new Token(TokenType.EndOfFile, string.Empty, _lineNumber, _column);
        }

        int startColumn = _column;
        int startLine = _lineNumber;

        // Peek first character without consuming
        int peekCode = _reader.Peek();
        if (peekCode == -1)
        {
            return new Token(TokenType.EndOfFile, string.Empty, _lineNumber, _column);
        }
        char peekChar = (char)peekCode;

        // 1. 空白字符 (including EndOfLine)
        if (char.IsWhiteSpace(peekChar))
        {
            // Consume the character now
            _reader.Read();
            _column++;

            if (peekChar == '\n')
            {
                _lineNumber++;
                _column = 1;
                return new Token(TokenType.EndOfLine, "\n", startLine, startColumn);
            }

            if (peekChar == '\r')
            {
                // Handle \r\n or \r
                if (!_reader.EndOfStream && _reader.Peek() == '\n')
                {
                    _reader.Read(); // 消耗 \n
                                    // Column already advanced for \r, don't advance again for \n here
                    _lineNumber++;
                    _column = 1;
                    return new Token(TokenType.EndOfLine, "\r\n", startLine, startColumn);
                }
                else
                {
                    _lineNumber++;
                    _column = 1;
                    return new Token(TokenType.EndOfLine, "\r", startLine, startColumn); // Standalone \r
                }
            }
            else // 其他空白 (space, tab)
            {
                var sb = new StringBuilder().Append(peekChar);
                while (!_reader.EndOfStream)
                {
                    int nextPeekCode = _reader.Peek();
                    if (nextPeekCode == -1) break;
                    char nextChar = (char)nextPeekCode;

                    if (char.IsWhiteSpace(nextChar) && nextChar != '\n' && nextChar != '\r')
                    {
                        _reader.Read(); // 消耗
                        _column++;
                        sb.Append(nextChar);
                    }
                    else
                    {
                        break;
                    }
                }
                return new Token(TokenType.Whitespace, sb.ToString(), startLine, startColumn);
            }
        }

        // 2. 注释 (// 到行尾)
        if (peekChar == '/')
        {
            // Consume '/'
            _reader.Read();
            _column++;
            if (!_reader.EndOfStream && _reader.Peek() == '/')
            {
                _reader.Read(); // 消耗第二个 '/'
                _column++;

                var sb = new StringBuilder("//");
                while (!_reader.EndOfStream)
                {
                    int nextPeekCode = _reader.Peek();
                    if (nextPeekCode == -1) break;
                    char nextChar = (char)nextPeekCode;

                    if (nextChar != '\n' && nextChar != '\r')
                    {
                        _reader.Read(); // 消耗
                        _column++;
                        sb.Append(nextChar);
                    }
                    else
                    {
                        break; // Stop before newline
                    }
                }
                return new Token(TokenType.Comment, sb.ToString(), startLine, startColumn);
            }
            else
            {
                // Handle single '/' (like division operator) later if needed
                // For now, treat single '/' as potential start of Unknown or OperatorDivide
                // if (peekChar == '/') { return new Token(TokenType.OperatorDivide, "/", startLine, startColumn); }
                throw new DslParserException($"行 {startLine}: Unexpected character '/'", startLine);
            }
        }


        // 3. 字符串字面量 (", ', `)
        if (peekChar == '"' || peekChar == '\'' || peekChar == '`')
        {
            char quoteChar = peekChar;
            _reader.Read(); // Consume opening quote
            _column++;
            var sb = new StringBuilder();
            bool isBacktick = quoteChar == '`';

            while (true)
            {
                int nextCode = _reader.Read();
                if (nextCode == -1) // EOF
                {
                    throw new DslParserException($"行 {startLine}: Unterminated string literal starting", startLine);
                }

                char nextChar = (char)nextCode;

                // Handle position tracking, especially for newlines
                if (nextChar == '\n')
                {
                    _column = 1;
                    _lineNumber++;
                }
                else if (nextChar == '\r')
                {
                    // Check for \r\n, consume \n if present
                    if (!_reader.EndOfStream && _reader.Peek() == '\n')
                    {
                        _reader.Read(); // Consume \n
                    }
                    _column = 1;
                    _lineNumber++;
                    nextChar = '\n'; // Normalize \r and \r\n to \n in the string value
                }
                else
                {
                    _column++;
                }


                if (nextChar == quoteChar)
                {
                    // Found closing quote
                    return new Token(TokenType.StringLiteral, sb.ToString(), startLine, startColumn);
                }

                if (!isBacktick && (nextChar == '\n' || nextChar == '\r'))
                {
                    // Use _lineNumber because it was updated *before* this check
                    throw new DslParserException($"行 {_lineNumber - 1}: Regular string literals (using \" or ') cannot span multiple lines. Use backticks (`) instead.", _lineNumber - 1);
                }

                if (nextChar == '\\') // Escape sequence
                {
                    if (_reader.EndOfStream) throw new DslParserException($"行 {_lineNumber}: Unterminated escape sequence at end of file", _lineNumber);
                    int escapedCharCode = _reader.Read();
                    if (escapedCharCode == -1) throw new DslParserException($"行 {_lineNumber}: Unterminated escape sequence at end of file", _lineNumber);

                    _column++; // Advance column for the escaped character  
                    ReadStringLiteral(sb, (char)escapedCharCode);
                }
                else
                {
                    sb.Append(nextChar);
                }
            }
        }


        // Consume the character for subsequent checks
        _reader.Read();
        char currentChar = peekChar; // Use the peeked char now that it's consumed
        _column++;


        // 4. 运算符 (after handling strings and comments)
        if (currentChar == '=')
        {
            if (!_reader.EndOfStream && _reader.Peek() == '=')
            {
                _reader.Read(); // 消耗第二个 '='
                _column++;
                return new Token(TokenType.OperatorEquals, "==", startLine, startColumn);
            }
            else // Single '=' is assignment
            {
                throw new DslParserException($"行 {startLine}: Unexpected character '='", startLine);
                // return new Token(TokenType.OperatorAssign, "=", startLine, startColumn);
            }
        }
        if (currentChar == '!')
        {
            if (!_reader.EndOfStream && _reader.Peek() == '=')
            {
                _reader.Read(); // 消耗 '='
                _column++;
                return new Token(TokenType.OperatorNotEquals, "!=", startLine, startColumn);
            }
            // Fall through to Unknown for single '!'
        }

        // Basic arithmetic operators
        //if (currentChar == '+') { return new Token(TokenType.OperatorPlus, "+", startLine, startColumn); }
        //if (currentChar == '-') { return new Token(TokenType.OperatorMinus, "-", startLine, startColumn); }
        //if (currentChar == '*') { return new Token(TokenType.OperatorMultiply, "*", startLine, startColumn); } 

        if (currentChar == '+' ||
            currentChar == '*' ||
            currentChar == '/')
        {
            // For now, treat them as Unknown or extend TokenType if needed
            return new Token(TokenType.Unknown, currentChar.ToString(), startLine, startColumn);
        }


        // 5. 括号, 逗号
        if (currentChar == '(') { return new Token(TokenType.ParenOpen, "(", startLine, startColumn); }
        if (currentChar == ')') { return new Token(TokenType.ParenClose, ")", startLine, startColumn); }
        if (currentChar == ',') { return new Token(TokenType.Comma, ",", startLine, startColumn); }

        // Backtick ` is handled by string literal logic now

        // 6. 变量 ($ followed by identifier rules)
        if (currentChar == '$')
        {
            // Check if the *next* char starts a valid identifier part
            if (_reader.EndOfStream || !IsValidIdentifierStart((char)_reader.Peek()))
            {
                throw new DslParserException($"行 {startLine}: Invalid character following '$', expected variable name", startLine);
            }


            var sb = new StringBuilder().Append(currentChar);
            while (!_reader.EndOfStream)
            {
                int nextPeekCode = _reader.Peek();
                if (nextPeekCode == -1) break;
                char nextChar = (char)nextPeekCode;

                if (IsValidIdentifierPart(nextChar))
                {
                    _reader.Read(); // 消耗
                    _column++;
                    sb.Append(nextChar);
                }
                else
                {
                    break;
                }
            }
            if (sb.Length == 1) // Just '$'
            {
                throw new DslParserException($"行 {startLine}: Invalid variable name starting with '$'", startLine);
            }
            return new Token(TokenType.Variable, sb.ToString(), startLine, startColumn);
        }

        // 7. 数字 (integer or decimal)
        if (currentChar == '-' || char.IsDigit(currentChar) || (currentChar == '.' && !_reader.EndOfStream && char.IsDigit((char)_reader.Peek())))
        {
            var sb = new StringBuilder();
            sb.Append(currentChar);

            bool hasDecimal = currentChar == '.';
            while (!_reader.EndOfStream)
            {
                int nextPeekCode = _reader.Peek();
                if (nextPeekCode == -1) break;
                char nextChar = (char)nextPeekCode;

                if (char.IsDigit(nextChar))
                {
                    _reader.Read(); // 消耗
                    _column++;
                    sb.Append(nextChar);
                }
                else if (nextChar == '.' && !hasDecimal)
                {
                    // Check if the char *after* the dot is a digit for valid decimal
                    _reader.Read(); // Consume '.'
                    _column++;
                    if (_reader.EndOfStream || !char.IsDigit((char)_reader.Peek()))
                    {
                        // Treat the '.' as belonging to the next token (or maybe error?)
                        // Backtrack? For now, let's just error or consider it end of number.
                        // For simplicity, let's stop the number here.
                        sb.Append('.'); // Include the dot we consumed
                        hasDecimal = true; // Mark it, although we stop after
                        break;
                        // Alternatively, throw: throw CreateException($"行 {startLine}: Invalid decimal number format", startLine);
                    }
                    sb.Append('.');
                    hasDecimal = true;
                }
                else
                {
                    break; // End of number
                }
            }
            // Check if it's just a single dot that was consumed
            if (sb.Length == 1 && sb[0] == '.')
            {
                // If single '.', treat as unknown, maybe it's part of something else
                return new Token(TokenType.Unknown, ".", startLine, startColumn);
            }
            return new Token(TokenType.Number, sb.ToString(), startLine, startColumn);
        }

        // 8. 标识符或关键字
        if (IsValidIdentifierStart(currentChar))
        {
            var sb = new StringBuilder();
            sb.Append(currentChar);

            while (!_reader.EndOfStream)
            {
                int nextPeekCode = _reader.Peek();
                if (nextPeekCode == -1) break;
                char nextChar = (char)nextPeekCode;

                if (IsValidIdentifierPart(nextChar))
                {
                    _reader.Read(); // 消耗
                    _column++;
                    sb.Append(nextChar);
                }
                else
                {
                    break;
                }
            }

            string value = sb.ToString();
            if (Keywords.TryGetValue(value, out var keywordType))
            {
                return new Token(keywordType, value, startLine, startColumn);
            }
            else
            {
                // Check if it's a known enum value or just a generic identifier
                if (System.Enum.TryParse<MouseAction>(value, true, out _) ||
                    System.Enum.TryParse<KeyboardAction>(value, true, out _) ||
                    System.Enum.TryParse<System.Windows.Forms.Keys>(value, true, out _))
                {
                    return new Token(TokenType.Identifier, value, startLine, startColumn);
                }
                // Assume it's a user-defined identifier if it follows the rules
                else if (Regexes.Identifier().IsMatch(value)) // Optional: stricter check if needed
                {
                    return new Token(TokenType.Identifier, value, startLine, startColumn);
                }
                else
                {
                    // Should not happen if IsValidIdentifierStart/Part are correct
                    throw new DslParserException($"行 {startLine}: Invalid identifier '{value}'", startLine);
                }
            }
        }

        // 9. Unknown character (already consumed)
        return new Token(TokenType.Unknown, currentChar.ToString(), startLine, startColumn);
    }

    private void ReadStringLiteral(StringBuilder sb, char escapedChar)
    {
        switch (escapedChar)
        {
            case '"': sb.Append('"'); break;
            case '`': sb.Append('`'); break;
            case 'n': sb.Append('\n'); break;
            case 'r': sb.Append('\r'); break;
            case 't': sb.Append('\t'); break;
            case '0': sb.Append('\0'); break;
            case '\'': sb.Append('\''); break;
            case '\\': sb.Append('\\'); break;
            case 'u':
                {
                    // Unicode escape \uXXXX
                    var hexSb = new StringBuilder();
                    for (int i = 0; i < 4; i++)
                    {
                        if (_reader.EndOfStream) throw new DslParserException($"行 {_lineNumber}: Incomplete Unicode escape sequence", _lineNumber);
                        int hexCharCode = _reader.Read();
                        if (hexCharCode == -1) throw new DslParserException($"行 {_lineNumber}: Incomplete Unicode escape sequence", _lineNumber);
                        _column++;
                        char hexChar = (char)hexCharCode;
                        if (!Uri.IsHexDigit(hexChar))
                        {
                            throw new DslParserException($"行 {_lineNumber}: Invalid character '{hexChar}' in Unicode escape sequence", _lineNumber);
                        }
                        hexSb.Append(hexChar);
                    }
                    string hexString = hexSb.ToString();
                    int codePoint = Convert.ToInt32(hexString, 16);
                    sb.Append((char)codePoint);
                }
                break;
            default:
                // Treat as literal backslash followed by the character
                sb.Append('\\').Append(escapedChar);
                break;
        }
    }

    private bool IsValidIdentifierStart(char c)
    {
        return char.IsLetter(c) || c == '_';
    }

    private bool IsValidIdentifierPart(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }

}