using MacroCreator.Models;

namespace MacroCreator.Utils;

public static class StringExtensions
{
    public static string Ellipsize(this string str, int maxLength)
    {
        if (str.Length <= maxLength) return str;
        if (maxLength <= 3) return new string('.', maxLength);
        return string.Concat(str.AsSpan(0, maxLength - 3), "...");
    }

    public static Keys ToKey(this char c)
    {
        return c switch
        {
            >= 'a' and <= 'z' => Keys.A + (c - 'a'),
            >= 'A' and <= 'Z' => (Keys.Shift | (Keys.A + (c - 'A'))),
            >= '0' and <= '9' => Keys.D0 + (c - '0'),
            ' ' => Keys.Space,
            '\t' => Keys.Tab,
            '\r' or '\n' => Keys.Enter,
            '.' => Keys.OemPeriod,
            ',' => Keys.Oemcomma,
            ';' => Keys.OemSemicolon,
            '\'' => Keys.OemQuotes,
            '/' => Keys.OemQuestion,
            '\\' => Keys.OemBackslash,
            '[' => Keys.OemOpenBrackets,
            ']' => Keys.OemCloseBrackets,
            '-' => Keys.OemMinus,
            '=' => Keys.Oemplus,
            '`' => Keys.Oemtilde,

            // 需要 Shift 的符号
            '!' => Keys.Shift | Keys.D1,
            '@' => Keys.Shift | Keys.D2,
            '#' => Keys.Shift | Keys.D3,
            '$' => Keys.Shift | Keys.D4,
            '%' => Keys.Shift | Keys.D5,
            '^' => Keys.Shift | Keys.D6,
            '&' => Keys.Shift | Keys.D7,
            '*' => Keys.Shift | Keys.D8,
            '(' => Keys.Shift | Keys.D9,
            ')' => Keys.Shift | Keys.D0,
            '_' => Keys.Shift | Keys.OemMinus,
            '+' => Keys.Shift | Keys.Oemplus,
            '{' => Keys.Shift | Keys.OemOpenBrackets,
            '}' => Keys.Shift | Keys.OemCloseBrackets,
            '|' => Keys.Shift | Keys.OemBackslash,
            ':' => Keys.Shift | Keys.OemSemicolon,
            '"' => Keys.Shift | Keys.OemQuotes,
            '<' => Keys.Shift | Keys.Oemcomma,
            '>' => Keys.Shift | Keys.OemPeriod,
            '?' => Keys.Shift | Keys.OemQuestion,
            '~' => Keys.Shift | Keys.Oemtilde,

            _ => Keys.None
        };
    }
}
