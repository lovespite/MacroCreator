using System.Text.RegularExpressions;

namespace MacroScript.Dsl;

internal partial class Regexes
{
    [GeneratedRegex(@"^\s*\$(?<var>[a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(?<expr>.*)$", RegexOptions.Compiled)]
    public static partial Regex VarAssignmentMatcher();

    [GeneratedRegex(@"\$(?<var>[a-zA-Z_][a-zA-Z0-9_]*)")]
    public static partial Regex VarReplacer();

    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]*$")]
    public static partial Regex Identifier();
}