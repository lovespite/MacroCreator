using MacroCreator.Models;
using System.Text.RegularExpressions;

namespace MacroScript.Dsl;

/// <summary>
/// 词法分析器，将 DSL 脚本字符串转换为 Token 序列
/// </summary>
public class Lexer
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
        { "custom", TokenType.KeywordCustom }
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

        // 1. 空白字符
        if (char.IsWhiteSpace(currentChar))
        {
            if (currentChar == '\n')
            {
                _position++;
                _lineNumber++;
                _column = 1;
                return new Token(TokenType.EndOfLine, "\n", _lineNumber - 1, startColumn);
            }
            if (currentChar == '\r') // 处理 \r\n 或 \r
            {
                _position++;
                _column++;
                if (_position < _script.Length && _script[_position] == '\n')
                {
                    _position++; // 跳过 \n
                }
                _lineNumber++;
                _column = 1;
                return new Token(TokenType.EndOfLine, "\r\n", _lineNumber - 1, startColumn);
            }
            else // 其他空白，如空格, tab
            {
                int startPos = _position;
                while (_position < _script.Length && char.IsWhiteSpace(_script[_position]) && _script[_position] != '\n' && _script[_position] != '\r')
                {
                    _position++;
                    _column++;
                }
                return new Token(TokenType.Whitespace, _script.Substring(startPos, _position - startPos), _lineNumber, startColumn);
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
            return new Token(TokenType.Comment, _script.Substring(startPos, _position - startPos), _lineNumber, startColumn);
        }

        // 3. 运算符 == 和 !=
        if (currentChar == '=')
        {
            if (_position + 1 < _script.Length && _script[_position + 1] == '=')
            {
                _position += 2;
                _column += 2;
                return new Token(TokenType.OperatorEquals, "==", _lineNumber, startColumn);
            }
        }
        if (currentChar == '!')
        {
            if (_position + 1 < _script.Length && _script[_position + 1] == '=')
            {
                _position += 2;
                _column += 2;
                return new Token(TokenType.OperatorNotEquals, "!=", _lineNumber, startColumn);
            }
        }

        // 4. 括号和逗号
        if (currentChar == '(')
        {
            _position++; _column++;
            return new Token(TokenType.ParenOpen, "(", _lineNumber, startColumn);
        }
        if (currentChar == ')')
        {
            _position++; _column++;
            return new Token(TokenType.ParenClose, ")", _lineNumber, startColumn);
        }
        if (currentChar == ',')
        {
            _position++; _column++;
            return new Token(TokenType.Comma, ",", _lineNumber, startColumn);
        }
        if (currentChar == '`') // Custom 表达式的反引号
        {
            _position++; _column++;
            int startPos = _position;
            while (_position < _script.Length && _script[_position] != '`')
            {
                if (_script[_position] == '\n' || _script[_position] == '\r') // 不允许跨行
                {
                    throw new DslParserException($"行 {_lineNumber}: Custom 表达式字符串不能跨越多行", _lineNumber);
                }
                _position++;
                _column++;
            }
            if (_position >= _script.Length) // 未找到结束的反引号
            {
                throw new DslParserException($"行 {_lineNumber}: 未找到 Custom 表达式的结束反引号 '`'", _lineNumber);
            }
            string value = _script.Substring(startPos, _position - startPos);
            _position++; _column++; // 跳过结束的反引号
                                    // 注意：这里返回 Identifier 类型，因为 Custom(`...`) 整体作为 Custom 函数的参数
            return new Token(TokenType.Identifier, value, _lineNumber, startColumn);
        }


        // 5. 数字 (整数或带小数)
        if (char.IsDigit(currentChar) || (currentChar == '.' && _position + 1 < _script.Length && char.IsDigit(_script[_position + 1])))
        {
            int startPos = _position;
            bool hasDecimal = false;
            while (_position < _script.Length && (char.IsDigit(_script[_position]) || (!hasDecimal && _script[_position] == '.')))
            {
                if (_script[_position] == '.') hasDecimal = true;
                _position++;
                _column++;
            }
            return new Token(TokenType.Number, _script.Substring(startPos, _position - startPos), _lineNumber, startColumn);
        }

        // 6. 标识符或关键字
        // (字母开头，后跟字母、数字或下划线)
        if (char.IsLetter(currentChar)) // 允许以下划线开头吗？目前不允许
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
                return new Token(keywordType, value, _lineNumber, startColumn);
            }
            else
            {
                // 检查是否为合法的枚举值（如 LeftDown, LControlKey 等）
                // 简单的检查，可以改进为更严格的验证
                if (System.Enum.TryParse<MouseAction>(value, true, out _) ||
                    System.Enum.TryParse<KeyboardAction>(value, true, out _) ||
                    System.Enum.TryParse<System.Windows.Forms.Keys>(value, true, out _))
                {
                    return new Token(TokenType.Identifier, value, _lineNumber, startColumn);
                }
                // 检查是否可能是标签名
                else if (Regex.IsMatch(value, @"^[a-zA-Z_][a-zA-Z0-9_]*$")) // 合法标签名/变量名
                {
                    return new Token(TokenType.Identifier, value, _lineNumber, startColumn);
                }
                else
                {
                    // 如果不是关键字也不是已知的枚举值或合法标识符，则标记为未知
                    // 或者，根据 DSL 的严格程度，可以抛出异常
                    // return new Token(TokenType.Unknown, value, _lineNumber, startColumn);
                    throw new DslParserException($"行 {_lineNumber}: 未识别的标识符 '{value}'", _lineNumber);
                }
            }
        }

        // 7. 未知字符
        _position++;
        _column++;
        return new Token(TokenType.Unknown, currentChar.ToString(), _lineNumber, startColumn);
    }
}
