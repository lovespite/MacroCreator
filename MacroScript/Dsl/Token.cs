namespace MacroScript.Dsl;

/// <summary>
/// 表示一个词法单元 (Token)
/// </summary>
public record Token(TokenType Type, string Value, int LineNumber, int Column);
