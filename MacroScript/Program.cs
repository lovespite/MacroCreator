using MacroCreator.Services.CH9329;
using MacroCreator.Utils;
using MacroScript.Dsl;
using MacroScript.Interactive;

namespace MacroScript;

internal static class Program
{
    private const string ArgNamePrefix = "--";
    private const string ArgNameAliasPrefix = "-";
    private static string[] _args = null!;
    public static string ConsoleTitle = null!;

    private static ConsoleHelper Console => ConsoleHelper.Instance;

    static async Task Main(string[] args)
    {
        _args = args;
        System.Console.Title = ConsoleTitle = "MacroScript_v1_" + Rnd.GetString(8);

        try
        {
            await ProcessCommand();
            // Console.PrintSuccess("Success");
        }
        catch (FileNotFoundException ex)
        {
            Console.PrintError($"文件未找到: {ex.FileName}");
        }
        catch (DslParserException ex)
        {
            Console.PrintError($"DSL 解析错误 (行 {ex.LineNumber}): {ex.Message}");
        }
        catch (CH9329Exception ex)
        {
            Console.PrintError($"CH9329 错误: {ex.ErrorCode} - {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            Console.PrintError($"超时错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.PrintError($"未知错误: {ex.Message}");
        }

        if (HasArg("pause", "p"))
            System.Console.ReadKey();
    }

    private static void PrintAllKeyNames(int columns = 10)
    {
        var names = Enum.GetNames<Keys>();
        Console.WriteLine("[");

        int colIndex = 0;

        for (var i = 0; i < names.Length; ++i)
        {
            Console.Write($"\"{names[i]}\", ");
            ++colIndex;
            if ((i + 1) % columns == 0)
            {
                Console.NextLine();
                colIndex = 0;
            }
        }

        Console.WriteLine("\n]");
    }

    private static async Task ProcessCommand()
    {
        if (_args.Length == 0)
        {
            await InteractiveMode(GetArg("redirect", "r"));
            return;
        }

        switch (_args[0])
        {
            case "interactive":
                {
                    await InteractiveMode(GetArg("redirect", "r"));
                }
                break;
            case "showkeys":
                {
                    PrintAllKeyNames();
                }
                break;
            case "compile":
                {
                    await CompileToFile(
                        inputFile: _args[1],
                        outputFile: GetArg("output", "o"),
                        viewInUiAfterCompiled: HasArg("view", "v")
                    );
                }
                break;
            case "run":
                {
                    await RunMacroScript(
                       inputFile: _args[1],
                       ch9329ComPort: GetArg("redirect", "r"),
                       hideConsole: HasArg("hide", "h")
                    );
                }
                break;
        }
    }

    public static bool HasArg(string name, string? alias = null)
    {
        var prefix = ArgNamePrefix + name;
        var altPrefix = alias is null ? null : ArgNameAliasPrefix + alias;

        return _args.Contains(prefix) || (altPrefix != null && _args.Contains(altPrefix));
    }

    public static string? GetArg(string name, string? alias = null)
    {
        var prefix = ArgNamePrefix + name;
        var altPrefix = alias is null ? null : ArgNameAliasPrefix + alias;

        for (var i = 0; i < _args.Length; ++i)
        {
            var a = _args[i];

            if (a == prefix || (altPrefix is not null && a == altPrefix))
            {
                if (i + 1 < _args.Length)
                    return _args[i + 1];
            }
        }

        return null;
    }

    public static nint HideConsoleWindow()
    {
        var handle = Utils.GetMainWindow();
        Utils.ShowWindow(handle, Utils.SW_HIDE);
        return handle;
    }

    public static void ShowConsoleWindow(nint handle)
    {
        Utils.ShowWindow(handle, Utils.SW_SHOW);
    }

    private static async Task InteractiveMode(string? ch9329ComPort)
    {
        var interf = new InteractiveInterface();
        using var interpreter = new InteractiveInterpreter().RegisterFunction(interf);

        if (ch9329ComPort is not null)
            await interf.Connect(ch9329ComPort);

        await interpreter.Start();
    }

    private static async Task CompileToFile(string inputFile, string? outputFile, bool viewInUiAfterCompiled)
    {
        using var interf = new InteractiveInterface();
        var output = await interf.Compile(inputFile, outputFile) as string;

        if (viewInUiAfterCompiled && File.Exists(output))
        {
            interf.View(output);
        }
    }

    private static async Task RunMacroScript(string inputFile, string? ch9329ComPort, bool hideConsole)
    {
        var handle = hideConsole ? HideConsoleWindow() : 0;
        try
        {
            using var interf = new InteractiveInterface();
            await interf.Connect(ch9329ComPort);
            await interf.RunMacroScriptFile(inputFile);
        }
        finally
        {
            if (handle != 0) ShowConsoleWindow(handle);
        }
    }
}
