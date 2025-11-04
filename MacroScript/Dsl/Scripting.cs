using MacroCreator.Models.Events;
using System.Text;

namespace MacroScript.Dsl;

internal static class Scripting
{
    public static List<MacroEvent> Compile(string filename)
    {
        using var lexer = new Lexer(filename);
        var tokens = lexer.Tokenize();
        var parser = new NewDslParser();
        var events = parser.Parse(tokens);
        return events;
    }

    public static Task<List<MacroEvent>> CompileAsync(string filename)
    {
        return Task.Run(() => Compile(filename));
    }

    public static List<MacroEvent> CompileFromString(string code)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(code));
        using var lexer = new Lexer(ms, Encoding.UTF8);
        var tokens = lexer.Tokenize();
        var parser = new NewDslParser();
        var events = parser.Parse(tokens);
        return events;
    }
}
