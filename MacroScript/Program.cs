using MacroCreator.Models.Events;
using MacroCreator.Services;
using MacroCreator.Services.CH9329;
using MacroCreator.Utils;
using MacroScript.Dsl;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Threading.Tasks;

namespace MacroScript;

internal static class Program
{
    private const string ArgNamePrefix = "--";
    private const string ArgNameAliasPrefix = "-";
    private static string[] _args = null!;
    public static string ConsoleTitle = null!;

    [STAThread]
    static void Main(string[] args)
    {
        Console.Title = ConsoleTitle = "MacroScript_v1_" + Rnd.GetString(8);
        _args = args;
        if (args.Length == 0) return;

        try
        {
            ProcessCommand().GetAwaiter().GetResult();
            Console.WriteLine("Success");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"文件未找到: {ex.FileName}");
        }
        catch (DslParserException ex)
        {
            Console.WriteLine($"DSL 解析错误 (行 {ex.LineNumber}): {ex.Message}");
        }
        catch (CH9329Exception ex)
        {
            Console.WriteLine($"CH9329 错误: {ex.ErrorCode} - {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"超时错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"未知错误: {ex.Message}");
        }

        if (HasArg("pause", "p"))
            Console.ReadKey();
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
                Console.WriteLine();
                colIndex = 0;
            }
        }

        Console.WriteLine("\n]");
    }

    private static async Task ProcessCommand()
    {
        var cmd = _args[0];
        switch (cmd)
        {
            case "showkeys":
                {
                    PrintAllKeyNames();
                }
                break;
            case "compile":
                {
                    var outputFile = CompileToFile(_args[1], GetArg("output", "o"));
                    if (HasArg("view", "v")) _ = OpenMacroFile(outputFile);
                }
                break;
            case "run":
                {
                    await RunMacroScript(_args[1]);
                }
                break;
        }
    }

    private static bool HasArg(string name, string? alias = null)
    {
        var prefix = ArgNamePrefix + name;
        var altPrefix = alias is null ? null : ArgNameAliasPrefix + alias;

        return _args.Contains(prefix) || (altPrefix != null && _args.Contains(altPrefix));
    }

    private static string? GetArg(string name, string? alias = null)
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

    private static string CompileToFile(string inputPath, string? outputPath)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("文件不存在", inputPath);
        var outputFile = outputPath ?? Path.ChangeExtension(inputPath, ".xml");

        FileService.Save(outputFile, Scripting.Compile(inputPath));

        return outputFile;
    }

    private static async Task<List<MacroEvent>> CompileAsync(string inputFile)
    {
        var tcs = new TaskCompletionSource<List<MacroEvent>>();

        var t = new Thread(() =>
        {
            try
            {
                var collection = Scripting.Compile(inputFile);

                if (collection.Count <= 0)
                    tcs.SetException(new InvalidDataException("事件序列为空"));
                else
                    tcs.SetResult(collection);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        t.IsBackground = true;
        t.Name = "CompileThread";
        t.Start();

        var eSeq = await tcs.Task;
        return eSeq;
    }

    private static Process? OpenMacroFile(string inputFile)
    {
        var psi = new ProcessStartInfo
        {
            FileName = nameof(MacroCreator) + ".exe",
            Arguments = $"open \"{inputFile}\"",
            UseShellExecute = false,
        };

        return Process.Start(psi);
    }

    private static async Task RunMacroScript(string inputFile)
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine("正在编译...");
        List<MacroEvent> eSeq = await CompileAsync(inputFile);
        var t = sw.Elapsed;
        Console.WriteLine($"编译完成, 用时 {t.TotalSeconds:0.00} s");

        var hWnd = HasArg("hide") ? HideCurrentConsoleWindow() : 0;

        // 检查 --redirect 参数
        var comPort = GetArg("redirect", "r");

        try
        {

            // 使用 using 语句确保 Controller 被释放 (从而释放 InputSimulator)
            using var controller = new MacroCreator.Controller.MacroController(eSeq, comPort);

            controller.OnPrint += ConsolePrinter.Instance.Print;
            controller.OnPrintLine += ConsolePrinter.Instance.PrintLine;

            if (controller.Redirected)
            {
                Console.WriteLine($"正在连接设备 {comPort} ...");
                var info = await controller.Simulator!.Controller.GetInfoAsync();
                Console.WriteLine($"已重定向到设备 {comPort}, 状态:\n{info}");
                if (info.UsbStatus == MacroCreator.Services.CH9329.UsbStatus.NotConnected)
                {
                    throw new InvalidOperationException("HID设备未连接到目标主机，或未被正确识别，请检查连接后重试。");
                }
            }

            Console.WriteLine("正在执行 ...");

            sw.Restart();
            await controller.StartPlayback();
            sw.Stop();

            t = sw.Elapsed;
            Console.WriteLine($"执行完成, 用时 {t.TotalSeconds:0.00} s");
        }
        finally
        {
            if (hWnd != 0) ShowCurrentConsoleWindow(hWnd);
        }
    }

    private static nint HideCurrentConsoleWindow()
    {
        var handle = Utils.GetMainWindow();
        Utils.ShowWindow(handle, Utils.SW_HIDE);
        return handle;
    }

    private static void ShowCurrentConsoleWindow(nint handle)
    {
        Utils.ShowWindow(handle, Utils.SW_SHOW);
    }
}
