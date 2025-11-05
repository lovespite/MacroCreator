using MacroCreator.Controller;
using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Services;
using MacroScript.Dsl;
using MacroScript.Utils;
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

    [InteractiveFunction(Name = "mv2", Description = "移动鼠标（绝对量）")]
    public async Task MouseMoveTo(int x, int y)
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.MouseMoveTo(x, y);
    }

    [InteractiveFunction(Name = "click", Description = "点击鼠标")]
    public async Task MouseClick(
        [InteractiveParameter(Description = "左键: Left(l)，右键: Right(r)，中键: Middle(m)，允许多个组合, eg. 同时按下左右键: click lr")] MouseButton button,
        [InteractiveParameter(Description = "按下释放按钮之间的延迟（毫秒）")] int delay = 50
    )
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.MouseClick(button, delay);
    }

    [InteractiveFunction(Name = "wheel", Description = "滚动滚轮")]
    public async Task MouseWheel(int amount)
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.MouseWheel(amount);
    }

    [InteractiveFunction(Name = "delay", Description = "延迟指定的毫秒数")]
    public async Task Delay(int milliseconds)
    {
        ThrowIfControllerNotConnected();
        await Task.Delay(milliseconds);
    }

    [InteractiveFunction(Name = "kd", Description = "按下指定按键（不释放）")]
    public async Task KeyDown(Keys[] keys)
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.KeyDown(keys);
    }

    [InteractiveFunction(Name = "ku", Description = "释放指定按键, 若未指定参数")]
    public async Task KeyUp(Keys[] keys)
    {
        ThrowIfControllerNotConnected();
        if (keys.Length == 0)
        {
            await _controller!.Simulator!.ReleaseAllKeys();
        }
        else
        {
            await _controller!.Simulator!.KeyUp(keys);
        }
    }

    [InteractiveFunction(Name = "type", Description = "输入文本,仅支持英文（包含大小写）、数字和基本符号")]
    public async Task TypeText(string text, int msDelay = 10)
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.TypeText(text, msDelay);
    }

    [InteractiveFunction(Name = "kua", Description = "释放所有按键")]
    public async Task ReleaseAllKeys()
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator!.ReleaseAllKeys();
    }

    [InteractiveFunction(Name = "press", Description = "按下并释放指定按键")]
    public async Task KeyPress(Keys[] keys, int delay = 50)
    {
        ThrowIfControllerNotConnected();
        foreach (var key in keys)
        {
            await _controller!.Simulator!.KeyPress(key, 20);
            await Task.Delay(delay);
        }
    }

    //[InteractiveFunction(Name = "combo", Description = "组合键按下并释放")]
    //public async Task Combine(Keys[] keys)
    //{
    //    ThrowIfControllerNotConnected();
    //    await _controller!.Simulator.KeyCombination(keys);
    //}

    [InteractiveFunction(Name = "combo", Description = "组合键按下并释放")]
    public async Task Combine(
        [InteractiveParameter(Description = "修饰键, Ctrl=$, Alt(%), Shift(^), Win(#), 可组合")] KeyModifier modifier,
        [InteractiveParameter(Description = "其他键")] Keys keys
    )
    {
        ThrowIfControllerNotConnected();
        await _controller!.Simulator.KeyCombination(modifier, keys);
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
