namespace MacroScript.Dsl;

// --- 词法分析 (Lexer) ---

/// <summary>
/// 定义 DSL 中的 Token 类型
/// </summary>
public enum TokenType
{
    // 控制流关键字
    KeywordIf, KeywordElse, KeywordEndIf, KeywordElseIf,
    KeywordWhile, KeywordEndWhile, KeywordBreak,
    KeywordFor, KeywordTo, KeywordEndFor, KeywordStep,
    KeywordLabel, KeywordGoto, KeywordExit,

    // 内置函数/命令关键字 (现在作为标识符处理)
    // KeywordDelay,
    // KeywordMouseDown, KeywordMouseUp, KeywordMouseClick,
    // KeywordMouseMove, KeywordMouseMoveTo, KeywordMouseWheel,
    // KeywordKeyDown, KeywordKeyUp, KeywordKeyPress,
    KeywordScript,

    // 条件关键字
    KeywordPixelColor, KeywordRGB, KeywordARGB, KeywordCustom,

    // 标记
    Identifier, // 标签名, 函数名, 枚举值等
    Variable, // $variable
    Number,

    // 运算符
    OperatorEquals, // = (赋值) 和 == (比较)
    OperatorCompareEquals, // ==
    OperatorNotEquals, // !=

    // 分隔符
    ParenOpen, ParenClose,
    Comma,
    StringLiteral, // Double-quoted, single-quoted, or backtick-quoted strings

    // 其他
    Comment,
    Whitespace,
    EndOfLine,
    EndOfFile,
    Unknown // 无法识别的 Token
}
