using MacroCreator.Services;

namespace MacroScript;

internal class ConsolePrinter : IPrintService
{
    public static ConsolePrinter Instance { get; } = new ConsolePrinter();

    public void Print(object? message)
    {
        Console.Write(message);
    }

    public void PrintLine(object? message)
    {
        Console.WriteLine(message);
    }
}
