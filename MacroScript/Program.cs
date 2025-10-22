using MacroCreator.Services;
using System.Diagnostics;

namespace MacroScript;

internal static class Program
{
    private const string ArgNamePrefix = "--";
    private const string ArgNameAliasPrefix = "-";
    private static string[] _args = null!;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        _args = args;
        if (args.Length < 2) return;

        try
        {
            ProcessCommand();
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
        catch
        {
            throw;
        }
    }

    private static void ProcessCommand()
    {
        var cmd = _args[0];
        switch (cmd)
        {
            case "compile":
                {
                    var outputFile = Compile(_args[1], GetArg("output", "o"));
                    if (HasArg("open")) _ = OpenMacroFile(outputFile);

                    break;
                }
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

    private static string Compile(string inputPath, string? outputPath)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("文件不存在", inputPath);

        var compiler = new Dsl.DslParser();
        var utf8Text = File.ReadAllText(inputPath);
        var outList = compiler.Parse(utf8Text);
        var outputFile = outputPath ?? Path.ChangeExtension(inputPath, ".xml");

        FileService.Save(outputFile, outList);

        return outputFile;
    }

    private static Process? OpenMacroFile(string file)
    {
        var psi = new ProcessStartInfo
        {
            FileName = nameof(MacroCreator) + ".exe",
            Arguments = $"open \"{file}\"",
            UseShellExecute = true
        };

        return Process.Start(psi);
    }
}