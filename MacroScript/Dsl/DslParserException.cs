namespace MacroScript.Dsl;

/// <summary>
/// 在 DSL 解析期间引发的特定异常。
/// </summary>
public class DslParserException : Exception
{
    public int LineNumber { get; }

    public DslParserException(string message, int lineNumber)
        : base(message)
    {
        LineNumber = lineNumber;
    }

    public DslParserException(string message, int lineNumber, Exception innerException)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
    }
}
