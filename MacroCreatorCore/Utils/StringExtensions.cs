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

    public static (KeyModifier, Keys) ToModifierAndKey(this char c)
    {
        return c switch
        {
            >= 'a' and <= 'z' => (KeyModifier.None, Keys.A + (c - 'a')),
            >= 'A' and <= 'Z' => (KeyModifier.LeftShift, Keys.A + (c - 'A')),
            >= '0' and <= '9' => (KeyModifier.None, Keys.D0 + (c - '0')),
            ' ' => (KeyModifier.None, Keys.Space),
            '\t' => (KeyModifier.None, Keys.Tab),
            '\r' or '\n' => (KeyModifier.None, Keys.Enter),
            '.' => (KeyModifier.None, Keys.OemPeriod),
            ',' => (KeyModifier.None, Keys.Oemcomma),
            ';' => (KeyModifier.None, Keys.OemSemicolon),
            '\'' => (KeyModifier.None, Keys.OemQuotes),
            '/' => (KeyModifier.None, Keys.OemQuestion),
            '\\' => (KeyModifier.None, Keys.OemBackslash),
            '[' => (KeyModifier.None, Keys.OemOpenBrackets),
            ']' => (KeyModifier.None, Keys.OemCloseBrackets),
            '-' => (KeyModifier.None, Keys.OemMinus),
            '=' => (KeyModifier.None, Keys.Oemplus),
            '`' => (KeyModifier.None, Keys.Oemtilde),

            // 需要 Shift 的符号
            '!' => (KeyModifier.LeftShift, Keys.D1),
            '@' => (KeyModifier.LeftShift, Keys.D2),
            '#' => (KeyModifier.LeftShift, Keys.D3),
            '$' => (KeyModifier.LeftShift, Keys.D4),
            '%' => (KeyModifier.LeftShift, Keys.D5),
            '^' => (KeyModifier.LeftShift, Keys.D6),
            '&' => (KeyModifier.LeftShift, Keys.D7),
            '*' => (KeyModifier.LeftShift, Keys.D8),
            '(' => (KeyModifier.LeftShift, Keys.D9),
            ')' => (KeyModifier.LeftShift, Keys.D0),
            '_' => (KeyModifier.LeftShift, Keys.OemMinus),
            '+' => (KeyModifier.LeftShift, Keys.Oemplus),
            '{' => (KeyModifier.LeftShift, Keys.OemOpenBrackets),
            '}' => (KeyModifier.LeftShift, Keys.OemCloseBrackets),
            '|' => (KeyModifier.LeftShift, Keys.OemBackslash),
            ':' => (KeyModifier.LeftShift, Keys.OemSemicolon),
            '"' => (KeyModifier.LeftShift, Keys.OemQuotes),
            '<' => (KeyModifier.LeftShift, Keys.Oemcomma),
            '>' => (KeyModifier.LeftShift, Keys.OemPeriod),
            '?' => (KeyModifier.LeftShift, Keys.OemQuestion),
            '~' => (KeyModifier.LeftShift, Keys.Oemtilde),

            _ => (KeyModifier.None, Keys.None)
        };
    }
}
