using MacroCreator.Controller;
using MacroCreator.Models.Events;
using MacroCreator.Services;
using MacroCreator.Services.CH9329;
using MacroScript.Dsl;
using System.Diagnostics;

namespace MacroScript.Interactive;

internal partial class InteractiveInterface : IDisposable
{
    private static readonly LruCache<string, List<MacroEvent>> _scriptCache = new();
    public MacroController? _controller;

    public InteractiveInterface() { }

    private void ThrowIfControllerNotConnected()
    {
        if (_controller is null)
            throw new InvalidOperationException("控制器未连接，请先使用 Connect 命令连接控制器。");
    }

    public void Dispose()
    {
        _controller?.Dispose();
    }

    [InteractiveFunction(Description = "连接控制器")]
    public async Task Connect([InteractiveParameter(Description = "CH9329端口（默认为空，使用本机控制器）")] string? ch9329ComPort = null)
    {
        if (_controller is not null)
        {
            try
            {
                ConsoleHelper.Instance.PrintWarning("正在释放当前控制器 ...");
                _controller.Dispose();
                _controller = null;
            }
            catch (Exception ex)
            {
                ConsoleHelper.Instance.PrintError($"释放控制器时发生错误: {ex.Message}");
            }
        }

        ConsoleHelper.Instance.PrintInfo($"正在创建{(ch9329ComPort is null ? "本机" : "CH9329")}控制器实例...");
        try
        {
            _controller = await GetMacroController(ch9329ComPort);
            ConsoleHelper.Instance.PrintSuccess("MacroController 实例创建成功。");
        }
        catch (Exception ex)
        {
            _controller?.Dispose();
            _controller = null;
            ConsoleHelper.Instance.PrintError($"创建控制器失败：{ex.Message}");
        }
    }

    [InteractiveFunction(Description = "播放XML序列文件")]
    public async Task Play([InteractiveParameter(Description = "文件路径")] string xmlFilePath)
    {
        ThrowIfControllerNotConnected();

        ConsoleHelper.Instance.PrintInfo($"正在加载文件: {xmlFilePath}");
        var sw = Stopwatch.StartNew();
        _controller!.LoadSequence(xmlFilePath);
        sw.Stop();
        ConsoleHelper.Instance.PrintSuccess($"加载完成，用时: {sw.Elapsed.TotalSeconds:0.00}");

        sw.Restart();
        ConsoleHelper.Instance.PrintLine("正在播放...");
        await _controller.StartPlayback();
        sw.Stop();

        ConsoleHelper.Instance.PrintSuccess($"播放完成，用时: {sw.Elapsed.TotalSeconds:0.00}");
    }

    [InteractiveFunction(Description = "编译DSL脚本文件为XML序列文件")]
    public async Task<object> Compile(
        [InteractiveParameter(Description = "输入文件路径")] string scriptFilePath,
        [InteractiveParameter(Description = "输出文件路径")] string? outputXmlFilePath = null
    )
    {
        var output = outputXmlFilePath ?? Path.ChangeExtension(scriptFilePath, ".xml");

        ConsoleHelper.Instance.PrintInfo($"正在编译: {Path.GetFileName(scriptFilePath)}");
        var sw = Stopwatch.StartNew();
        var collection = await CompileAsync(scriptFilePath);
        sw.Stop();
        ConsoleHelper.Instance.PrintSuccess($"编译完成，用时: {sw.Elapsed.TotalSeconds:0.00}");

        ConsoleHelper.Instance.PrintInfo($"正在写入文件 {Path.GetFileName(output)}");
        sw.Restart();
        FileService.Save(output, collection);
        sw.Stop();

        ConsoleHelper.Instance.PrintSuccess($"已保存，用时 {sw.Elapsed.TotalSeconds:0.00}");

        return output;
    }

    [InteractiveFunction(Description = "在MacroCreator UI中查看事件序列XML文件")]
    public void View([InteractiveParameter(Description = "文件路径")] string xmlFilePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = nameof(MacroCreator) + ".exe",
            Arguments = $"open \"{xmlFilePath}\"",
            UseShellExecute = true,
        };

        Process.Start(psi);
    }

    [InteractiveFunction(Name = "run", Description = "运行宏脚本（*.macroscript）文件")]
    public async Task RunMacroScriptFile([InteractiveParameter(Description = "脚本文件路径")] string scriptFilePath)
    {
        ThrowIfControllerNotConnected();

        var sw = Stopwatch.StartNew();
        ConsoleHelper.Instance.PrintInfo("正在编译 ...");
        List<MacroEvent> eSeq = await CompileAsync(scriptFilePath);
        var t = sw.Elapsed;
        ConsoleHelper.Instance.PrintSuccess($"编译完成, 用时 {t.TotalSeconds:0.00} s");

        ConsoleHelper.Instance.PrintInfo("正在执行 ...");

        sw.Restart();
        await _controller!.StartPlayback(eSeq);
        sw.Stop();

        t = sw.Elapsed;
        ConsoleHelper.Instance.PrintSuccess($"执行完成, 用时 {t.TotalSeconds:0.00} s");
    }

    [InteractiveFunction(Name = "execute", Description = "运行宏脚本片段")]
    public async Task RunMacroScript([InteractiveParameter(Description = "脚本代码")] string script)
    {
        ThrowIfControllerNotConnected();

        if (_scriptCache.TryGet(script, out var eSeq))
        {
            ConsoleHelper.Instance.PrintLine("使用脚本缓存，跳过编译");
        }
        else
        {
            var swCompile = Stopwatch.StartNew();
            ConsoleHelper.Instance.PrintInfo("正在编译...");
            eSeq = Scripting.CompileFromString(script);
            swCompile.Stop();
            var tCompile = swCompile.Elapsed;
            ConsoleHelper.Instance.PrintSuccess($"编译完成, 用时 {tCompile.TotalSeconds:0.00} s");

            _scriptCache.Set(script, eSeq);
        }

        ConsoleHelper.Instance.PrintInfo("正在执行 ...");

        var sw = Stopwatch.StartNew();
        await _controller!.StartPlayback(eSeq);
        sw.Stop();

        ConsoleHelper.Instance.PrintSuccess($"执行完成, 用时 {sw.Elapsed.TotalSeconds:0.00} s");
    }

    [InteractiveFunction(Name = "mv", Description = "移动鼠标（相对量，±127）")]
    public async Task MouseMove(int dx, int dy)
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.MouseMove(dx, dy);
    }

    [InteractiveFunction(Name = "click", Alias = "tap", Description = "点击鼠标")]
    public async Task MouseClick(
        [InteractiveParameter(Description = "按下的键\nLeft(l): 左键\nRight(r): 右键\nMiddle(m): 中键\n单个或组合, 例如: click lr // 同时按下左右键")] MouseButton button,
        [InteractiveParameter(Description = "按下释放按钮之间的延迟（毫秒）")] int delay = 50
    )
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.MouseClick(button, delay);
    }

#if DEBUG
    [InteractiveFunction(Description = "测试方法")]
    public Task<object?> Test(
        [InteractiveParameter(Description = "测试参数-字符串")] string filename,
        [InteractiveParameter(Description = "测试参数-Int32")] int testParam = 3
    )
    {
        return Task.FromResult((object?)$"{filename} {testParam}");
    }
#endif

}

partial class InteractiveInterface
{
    public static async Task<MacroController> GetMacroController(string? comPort)
    {
        var simulator = comPort is null ? null : InputSimulator.Open(comPort);
        simulator?.Controller.Open();
        simulator?.Controller.WarmupCache();
        var controller = new MacroController(simulator);

        controller.OnPrint += ConsoleHelper.Instance.Print;
        controller.OnPrintLine += ConsoleHelper.Instance.PrintLine;

        if (controller.Redirected)
        {
            ConsoleHelper.Instance.PrintLine($"正在连接设备 {comPort} ...");
            var info = await controller.Simulator!.Controller.GetInfoAsync();
            ConsoleHelper.Instance.PrintInfo($"已重定向到设备 {comPort}, 状态:\n{info}");
            if (info.UsbStatus == UsbStatus.NotConnected)
            {
                throw new InvalidOperationException("HID设备未连接到目标主机，或未被正确识别，请检查连接后重试。");
            }
        }

        return controller;
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

        return await tcs.Task;
    }

    internal static void InstallCustomParameterDeserializers()
    {
        ParameterDeserializer.SetCustomDeserializer(typeof(MouseButton), s =>
        {
            if (Enum.TryParse<MouseButton>(s, true, out var btn)) return btn;
            s = s.ToLowerInvariant();
            if (string.Equals(s, "all", StringComparison.OrdinalIgnoreCase))
            {
                return MouseButton.Left | MouseButton.Right | MouseButton.Middle;
            }

            if (int.TryParse(s, out var intVal))
            {
                return (MouseButton)intVal;
            }

            MouseButton flag = MouseButton.None;
            if (s.Contains('l'))
                flag |= MouseButton.Left;
            if (s.Contains('r'))
                flag |= MouseButton.Right;
            if (s.Contains('m'))
                flag |= MouseButton.Middle;

            return flag;

        });
    }
}
