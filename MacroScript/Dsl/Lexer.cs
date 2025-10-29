using MacroCreator.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace MacroScript.Dsl;

/// <summary>
/// 词法分析器，将 DSL 脚本字符串转换为 Token 序列
/// </summary>
public partial class Lexer : IDisposable
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
        { "mouse", TokenType.KeywordMouse },
        { "key", TokenType.KeywordKey },
        { "pixelcolor", TokenType.KeywordPixelColor },
        { "rgb", TokenType.KeywordRGB },
        { "argb", TokenType.KeywordARGB },
        { "custom", TokenType.KeywordCustom },
        { "script", TokenType.KeywordScript },
        { "endscript", TokenType.KeywordEndScript },
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
            if (token.Type == TokenType.EndOfFile)
            {
                break;
            }

            // --- 关键修改 ---
            // 过滤掉空白 (space/tab) 和注释。
            // 根据 NextToken() 的逻辑, EndOfLine 是一个独立的类型, 不是 Whitespace。
            // 因此, 这个条件会保留 EndOfLine (以及所有其他有效 Token)，并且只返回一次。
            if (token.Type != TokenType.Whitespace && token.Type != TokenType.Comment)
            {
                yield return token;
            }
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

        // 1. 读取并消耗下一个字符
        int charCode = _reader.Read();
        if (charCode == -1)
        {
            return new Token(TokenType.EndOfFile, string.Empty, _lineNumber, _column);
        }

        char currentChar = (char)charCode;
        _column++; // 预先增加列号

        // 1. 空白字符 (including EndOfLine)
        if (char.IsWhiteSpace(currentChar))
        {
            if (currentChar == '\n')
            {
                _lineNumber++;
                _column = 1;
                return new Token(TokenType.EndOfLine, "\n", startLine, startColumn);
            }
            if (currentChar == '\r')
            {
                // 处理 \r\n 或 \r
                if (!_reader.EndOfStream && _reader.Peek() == '\n')
                {
                    _reader.Read(); // 消耗 \n
                }
                _lineNumber++;
                _column = 1;
                return new Token(TokenType.EndOfLine, "\r\n", startLine, startColumn);
            }
            else // 其他空白 (space, tab)
            {
                var sb = new StringBuilder().Append(currentChar);
                while (!_reader.EndOfStream)
                {
                    char nextChar = (char)_reader.Peek();
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
        if (currentChar == '/' && !_reader.EndOfStream && _reader.Peek() == '/')
        {
            _reader.Read(); // 消耗第二个 '/'
            _column++;

            var sb = new StringBuilder("//");
            while (!_reader.EndOfStream)
            {
                char nextChar = (char)_reader.Peek();
                if (nextChar != '\n' && nextChar != '\r')
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
            return new Token(TokenType.Comment, sb.ToString(), startLine, startColumn);
        }

        // 3. 运算符
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
                return new Token(TokenType.OperatorAssign, "=", startLine, startColumn);
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
            // 注意：单独的 '!' 会落到 Unknown
        }

        // Basic arithmetic operators
        if (currentChar == '+') { return new Token(TokenType.OperatorPlus, "+", startLine, startColumn); }
        if (currentChar == '-') { return new Token(TokenType.OperatorMinus, "-", startLine, startColumn); }
        if (currentChar == '*') { return new Token(TokenType.OperatorMultiply, "*", startLine, startColumn); }
        if (currentChar == '/') { return new Token(TokenType.OperatorDivide, "/", startLine, startColumn); } // // 已被处理


        // 4. 括号, 逗号, 反引号
        if (currentChar == '(') { return new Token(TokenType.ParenOpen, "(", startLine, startColumn); }
        if (currentChar == ')') { return new Token(TokenType.ParenClose, ")", startLine, startColumn); }
        if (currentChar == ',') { return new Token(TokenType.Comma, ",", startLine, startColumn); }
        if (currentChar == '`') // Custom expression backtick
        {
            var sb = new StringBuilder();
            while (true) // 循环直到找到 '`' 或 EOF
            {
                int nextCode = _reader.Read();
                if (nextCode == -1) // EOF
                {
                    throw new DslParserException($"行 {startLine}: Unterminated Custom expression string starting", startLine);
                }

                char nextChar = (char)nextCode;
                _column++;

                if (nextChar == '`')
                {
                    // 找到结束的反引号
                    return new Token(TokenType.Identifier, sb.ToString(), startLine, startColumn);
                }

                if (nextChar == '\n' || nextChar == '\r')
                {
                    // 在抛出异常前更新行号/列号
                    if (nextChar == '\r' && !_reader.EndOfStream && _reader.Peek() == '\n')
                    {
                        _reader.Read(); // 消耗 \n
                    }
                    _lineNumber++;
                    _column = 1;
                    throw new DslParserException($"行 {startLine}: Custom expression string cannot span multiple lines", startLine);
                }

                sb.Append(nextChar);
            }
        }

        // 5. 变量 ($ followed by identifier rules)
        if (currentChar == '$')
        {
            if (_reader.EndOfStream || !char.IsLetter((char)_reader.Peek()))
            {
                throw new DslParserException($"行 {startLine}: Invalid variable name starting", startLine);
            }

            var sb = new StringBuilder().Append(currentChar);
            while (!_reader.EndOfStream)
            {
                char nextChar = (char)_reader.Peek();
                if (char.IsLetterOrDigit(nextChar) || nextChar == '_')
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
            return new Token(TokenType.Variable, sb.ToString(), startLine, startColumn);
        }

        // 6. 数字 (integer or decimal)
        if (char.IsDigit(currentChar) || (currentChar == '.' && !_reader.EndOfStream && char.IsDigit((char)_reader.Peek())))
        {
            var sb = new StringBuilder();
            sb.Append(currentChar);

            bool hasDecimal = currentChar == '.';
            while (!_reader.EndOfStream)
            {
                char nextChar = (char)_reader.Peek();

                if (char.IsDigit(nextChar))
                {
                    _reader.Read(); // 消耗
                    _column++;
                    sb.Append(nextChar);
                }
                else if (nextChar == '.' && !hasDecimal)
                {
                    _reader.Read(); // 消耗
                    _column++;
                    sb.Append(nextChar);
                    hasDecimal = true;
                }
                else
                {
                    break; // End of number
                }
            }
            return new Token(TokenType.Number, sb.ToString(), startLine, startColumn);
        }

        // 7. 标识符或关键字
        if (char.IsLetter(currentChar))
        {
            var sb = new StringBuilder();
            sb.Append(currentChar);

            while (!_reader.EndOfStream)
            {
                char nextChar = (char)_reader.Peek();
                if (char.IsLetterOrDigit(nextChar) || nextChar == '_')
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
                // 检查是否为已知的 enum 值或有效的通用标识符
                if (System.Enum.TryParse<MouseAction>(value, true, out _) ||
                    System.Enum.TryParse<KeyboardAction>(value, true, out _) ||
                    System.Enum.TryParse<System.Windows.Forms.Keys>(value, true, out _) ||
                    Regex_VariableName().IsMatch(value))
                {
                    return new Token(TokenType.Identifier, value, startLine, startColumn);
                }
                else
                {
                    throw new DslParserException($"行 {startLine}: Unrecognized identifier '{value}'", startLine);
                }
            }
        }

        // 8. Unknown character
        // 字符已被消耗，_column 已被增加
        return new Token(TokenType.Unknown, currentChar.ToString(), startLine, startColumn);
    }

    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex Regex_VariableName();
}