using MacroCreator.Models.Events;
using MacroCreator.Services;
using MacroCreator.Utils;
using MacroScript.Dsl;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MacroScript;

internal static class Program
{
    private const string ArgNamePrefix = "--";
    private const string ArgNameAliasPrefix = "-";
    private static string[] _args = null!;
    public static string ConsoleTitle = null!;

    static void Main(string[] args)
    {
        Console.Title = ConsoleTitle = "MacroScript_v1_" + Rnd.GetString(8);
        _args = args;
        if (args.Length < 2) return;

        try
        {
            ProcessCommand().GetAwaiter().GetResult();
            Console.WriteLine("Success");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"文件未找到: {ex.FileName}");
        }
        catch (Dsl.DslParserException ex)
        {
            Console.WriteLine($"DSL 解析错误 (行 {ex.LineNumber}): {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"未知错误: {ex.Message}");
        }

        if (HasArg("pause", "p"))
            Console.ReadKey();
    }

    private static async Task ProcessCommand()
    {
        var cmd = _args[0];
        switch (cmd)
        {
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

        return _args.Contains(prefix) || _args.Contains(altPrefix);
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

        var utf8Text = File.ReadAllText(inputPath);
        FileService.Save(outputFile, Scripting.Compile(utf8Text));

        return outputFile;
    }

    private static async Task<List<MacroEvent>> CompileAsync(string inputFile)
    {
        var tcs = new TaskCompletionSource<List<MacroEvent>>();

        var t = new Thread(() =>
        {
            try
            {
                var utf8Text = File.ReadAllText(inputFile);
                var collection = Scripting.Compile(utf8Text);

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

        var hWnd = HideCurrentConsoleWindow();

        try
        {
            Console.WriteLine("正在执行...");
            var controller = new MacroCreator.Controller.MacroController(eSeq);
            sw.Restart();
            await controller.StartPlayback();
            sw.Stop();
            t = sw.Elapsed;
            Console.WriteLine($"执行完成, 用时 {t.TotalSeconds:0.00} s");
        }
        finally
        {
            ShowCurrentConsoleWindow(hWnd);
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

internal static partial class Utils
{

    public static nint GetMainWindow()
    {
        // "CASCADIA_HOSTING_WINDOW_CLASS"
        return FindWindowA(null, Program.ConsoleTitle);
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(IntPtr hWnd, uint nCmdShow);
    public const uint SW_HIDE = 0;
    public const uint SW_SHOW = 5;

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint FindWindowA(string? lpClassName, string lpWindowName);


    //[LibraryImport("user32.dll", SetLastError = true)]
    //private static partial int GetClassNameA(IntPtr hWnd, Span<byte> lpClassName, int nMaxCount);

    //[LibraryImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static partial bool FreeConsole();

    //[LibraryImport("kernel32.dll", SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static partial bool AllocConsole();

    //public static string GetWindowClassName(IntPtr hWnd)
    //{
    //    Span<byte> buffer = stackalloc byte[256];
    //    int length = GetClassNameA(hWnd, buffer, buffer.Length);
    //    if (length > 0)
    //    {
    //        return System.Text.Encoding.UTF8.GetString(buffer[..length]);
    //    }
    //    return string.Empty;
    //}

    //[LibraryImport("user32.dll", SetLastError = true)]
    //private static partial int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
    //public static int GetWindowProcessId(IntPtr hWnd)
    //{
    //    GetWindowThreadProcessId(hWnd, out int pid);
    //    return pid;
    //}
}