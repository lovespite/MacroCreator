using MacroCreator.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroScript.Dsl;

internal static class Scripting
{
    public static List<MacroEvent> Compile(string filename)
    {
        var lexer = new Lexer(filename);
        var tokens = lexer.Tokenize();
        var parser = new NewDslParser();
        var events = parser.Parse(tokens);
        return events;
    }

    public static Task<List<MacroEvent>> CompileAsync(string filename)
    {
        return Task.Run(() => Compile(filename));
    }
}
