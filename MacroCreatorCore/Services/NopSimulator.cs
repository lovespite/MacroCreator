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
    public override Task KeyDown(params Keys[] key) => Task.CompletedTask;
    public override Task KeyUp(params Keys[] key) => Task.CompletedTask;
    public override Task ReleaseAllKeys() => Task.CompletedTask;
    public override Task ReleaseAllMouse() => Task.CompletedTask;
    public override void SetScreenResolution(int width, int height) { }
    public override void Dispose() { }
}