using MacroCreator.Models;
using System.Text.RegularExpressions;

namespace MacroScript.Dsl;

/// <summary>
/// 词法分析器，将 DSL 脚本字符串转换为 Token 序列
/// </summary>
public partial class Lexer
{
    private readonly string _script;
    private int _position;
    private int _lineNumber;
    private int _column;

    // 关键字映射
    private static readonly Dictionary<string, TokenType> Keywords = new(System.StringComparer.OrdinalIgnoreCase)
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

    public Lexer(string script)
    {
        _script = script;
        _position = 0;
        _lineNumber = 1;
        _column = 1;
    }

    /// <summary>
    /// 生成所有 Token
    /// </summary>
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (_position < _script.Length)
        {
            var token = NextToken();
            if (token.Type != TokenType.Whitespace && token.Type != TokenType.Comment)
            {
                tokens.Add(token);
            }
            if (token.Type == TokenType.EndOfLine)
            {
                tokens.Add(token); // 保留换行符，有助于解析语句边界和错误报告
            }
        }
        tokens.Add(new Token(TokenType.EndOfFile, string.Empty, _lineNumber, _column));
        return tokens;
    }

    /// <summary>
    /// 获取下一个 Token
    /// </summary>
    private Token NextToken()
    {
        if (_position >= _script.Length)
        {
            return new Token(TokenType.EndOfFile, string.Empty, _lineNumber, _column);
        }

        char currentChar = _script[_position];
        int startColumn = _column;
        int startLine = _lineNumber; // Store line number at the start of the token

        // 1. 空白字符 (including EndOfLine)
        if (char.IsWhiteSpace(currentChar))
        {
            if (currentChar == '\n')
            {
                _position++;
                _lineNumber++;
                _column = 1;
                return new Token(TokenType.EndOfLine, "\n", startLine, startColumn);
            }
            if (currentChar == '\r') // Handle \r\n or \r
            {
                _position++;
                _column++;
                if (_position < _script.Length && _script[_position] == '\n')
                {
                    _position++; // Skip \n
                }
                _lineNumber++;
                _column = 1;
                // Report EndOfLine on the line where \r started
                return new Token(TokenType.EndOfLine, "\r\n", startLine, startColumn);
            }
            else // Other whitespace (space, tab)
            {
                int startPos = _position;
                while (_position < _script.Length && char.IsWhiteSpace(_script[_position]) && _script[_position] != '\n' && _script[_position] != '\r')
                {
                    _position++;
                    _column++;
                }
                return new Token(TokenType.Whitespace, _script.Substring(startPos, _position - startPos), startLine, startColumn);
            }
        }

        // 2. 注释 (// 到行尾)
        if (currentChar == '/' && _position + 1 < _script.Length && _script[_position + 1] == '/')
        {
            int startPos = _position;
            while (_position < _script.Length && _script[_position] != '\n' && _script[_position] != '\r')
            {
                _position++;
                _column++;
            }
            return new Token(TokenType.Comment, _script.Substring(startPos, _position - startPos), startLine, startColumn);
        }

        // 3. 运算符
        if (currentChar == '=')
        {
            if (_position + 1 < _script.Length && _script[_position + 1] == '=')
            {
                _position += 2; _column += 2;
                return new Token(TokenType.OperatorEquals, "==", startLine, startColumn);
            }
            else // Single '=' is assignment
            {
                _position++; _column++;
                return new Token(TokenType.OperatorAssign, "=", startLine, startColumn);
            }
        }
        if (currentChar == '!')
        {
            if (_position + 1 < _script.Length && _script[_position + 1] == '=')
            {
                _position += 2; _column += 2;
                return new Token(TokenType.OperatorNotEquals, "!=", startLine, startColumn);
            }
        }
        // Basic arithmetic operators
        if (currentChar == '+') { _position++; _column++; return new Token(TokenType.OperatorPlus, "+", startLine, startColumn); }
        if (currentChar == '-') { _position++; _column++; return new Token(TokenType.OperatorMinus, "-", startLine, startColumn); }
        if (currentChar == '*') { _position++; _column++; return new Token(TokenType.OperatorMultiply, "*", startLine, startColumn); }
        if (currentChar == '/') { _position++; _column++; return new Token(TokenType.OperatorDivide, "/", startLine, startColumn); }


        // 4. 括号, 逗号, 反引号
        if (currentChar == '(') { _position++; _column++; return new Token(TokenType.ParenOpen, "(", startLine, startColumn); }
        if (currentChar == ')') { _position++; _column++; return new Token(TokenType.ParenClose, ")", startLine, startColumn); }
        if (currentChar == ',') { _position++; _column++; return new Token(TokenType.Comma, ",", startLine, startColumn); }
        if (currentChar == '`') // Custom expression backtick
        {
            _position++; _column++;
            int startPos = _position;
            while (_position < _script.Length && _script[_position] != '`')
            {
                if (_script[_position] == '\n' || _script[_position] == '\r')
                {
                    throw new DslParserException($"行 {startLine}: Custom expression string cannot span multiple lines", startLine);
                }
                _position++;
                _column++;
            }
            if (_position >= _script.Length)
            {
                throw new DslParserException($"行 {startLine}: Unterminated Custom expression string starting", startLine);
            }
            string value = _script.Substring(startPos, _position - startPos);
            _position++; _column++; // Skip closing backtick
                                    // Return as Identifier, used as argument to Custom()
            return new Token(TokenType.Identifier, value, startLine, startColumn);
        }

        // 5. 变量 ($ followed by identifier rules)
        if (currentChar == '$')
        {
            _position++; _column++;
            if (_position >= _script.Length || !char.IsLetter(_script[_position])) // Must start with a letter after $
            {
                throw new DslParserException($"行 {startLine}: Invalid variable name starting", startLine);
            }
            int startPos = _position;
            while (_position < _script.Length && (char.IsLetterOrDigit(_script[_position]) || _script[_position] == '_'))
            {
                _position++;
                _column++;
            }
            string varName = _script.Substring(startPos, _position - startPos);
            return new Token(TokenType.Variable, varName, startLine, startColumn); // Store only the name, not $
        }

        // 6. 数字 (integer or decimal)
        if (char.IsDigit(currentChar) || (currentChar == '.' && _position + 1 < _script.Length && char.IsDigit(_script[_position + 1])))
        {
            int startPos = _position;
            bool hasDecimal = currentChar == '.';
            while (_position < _script.Length)
            {
                char nextChar = _script[_position];
                if (char.IsDigit(nextChar))
                {
                    _position++; _column++;
                }
                else if (nextChar == '.' && !hasDecimal)
                {
                    hasDecimal = true;
                    _position++; _column++;
                }
                else
                {
                    break; // End of number
                }
            }
            return new Token(TokenType.Number, _script.Substring(startPos, _position - startPos), startLine, startColumn);
        }

        // 7. 标识符或关键字 (letter followed by letter, digit, or underscore)
        if (char.IsLetter(currentChar))
        {
            int startPos = _position;
            while (_position < _script.Length && (char.IsLetterOrDigit(_script[_position]) || _script[_position] == '_'))
            {
                _position++;
                _column++;
            }
            string value = _script.Substring(startPos, _position - startPos);
            if (Keywords.TryGetValue(value, out var keywordType))
            {
                return new Token(keywordType, value, startLine, startColumn);
            }
            else
            {
                // Check if it's a known enum value or a valid general identifier
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
        _position++;
        _column++;
        return new Token(TokenType.Unknown, currentChar.ToString(), startLine, startColumn);
    }

    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex Regex_VariableName();
}
