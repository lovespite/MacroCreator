namespace MacroScript.Dsl;

// --- 词法分析 (Lexer) ---

/// <summary>
/// 定义 DSL 中的 Token 类型
/// </summary>
public enum TokenType
{
    KeywordIf, KeywordElse, KeywordEndIf,
    KeywordWhile, KeywordEndWhile, KeywordBreak,
    KeywordLabel, KeywordGoto, KeywordExit,
    KeywordDelay, KeywordMouse, KeywordKey,
    KeywordPixelColor, KeywordRGB, KeywordARGB, KeywordCustom, // 条件相关关键字
    KeywordScript,
    Identifier, // 标签名, 事件名, 枚举值等
    Variable, // $variable
    Number,
    OperatorAssign, // =
    OperatorPlus, OperatorMinus, OperatorMultiply, OperatorDivide, // Basic operators for expressions
    OperatorEquals, OperatorNotEquals,
    ParenOpen, ParenClose,
    Comma,
    // Backtick, // Backtick is removed as it's now part of StringLiteral handling
    StringLiteral, // Double-quoted, single-quoted, or backtick-quoted strings
    Comment,
    Whitespace,
    EndOfLine,
    EndOfFile,
    Unknown // 无法识别的 Token
}
