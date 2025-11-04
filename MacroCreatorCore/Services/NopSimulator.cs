/*
 * CH9329 输入模拟器 - 高级封装类
 * 
 * 提供简化的鼠标和键盘操作接口，基于 Ch9329Controller 实现。
 */

using MacroCreator.Models;

namespace MacroCreator.Services;

public class NopSimulator : SimulatorBase
{
    public static readonly NopSimulator Instance = new();

    public override string Name => "NopSimulator";
    public override Task MouseMove(int dx, int dy) => Task.CompletedTask;
    public override Task MouseMoveTo(int x, int y) => Task.CompletedTask;
    public override Task MouseDown(MouseButton button) => Task.CompletedTask;
    public override Task MouseUp(MouseButton button) => Task.CompletedTask;
    public override Task MouseWheel(int amount) => Task.CompletedTask;
    public override Task KeyDown(Keys key) => Task.CompletedTask;
    public override Task KeyUp(Keys key) => Task.CompletedTask;
    public override Task ReleaseAllKeys() => Task.CompletedTask;
    public override Task ReleaseAllMouse() => Task.CompletedTask;
    public override void SetScreenResolution(int width, int height) { }
    public override void Dispose() { }
}