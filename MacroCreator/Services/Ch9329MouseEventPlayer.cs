using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Services.CH9329;

namespace MacroCreator.Services;

/// <summary>
/// 使用 CH9329 InputSimulator 回放鼠标事件的播放器
/// </summary>
public class Ch9329MouseEventPlayer : IEventPlayer
{
    private readonly InputSimulator _simulator;

    public Ch9329MouseEventPlayer(InputSimulator simulator)
    {
        _simulator = simulator;
    }

    public async Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        if (context.CurrentEvent is not MouseEvent me)
        {
            return PlaybackResult.Continue();
        }

        switch (me.Action)
        {
            case MouseAction.Move:
                await _simulator.MouseMove(me.X, me.Y);
                break;
            case MouseAction.MoveTo:
                await _simulator.MouseMoveTo(me.X, me.Y);
                break;
            case MouseAction.LeftDown:
                await _simulator.MouseDown(MouseButton.Left);
                break;
            case MouseAction.LeftUp:
                await _simulator.MouseUp(MouseButton.Left);
                break;
            case MouseAction.RightDown:
                await _simulator.MouseDown(MouseButton.Right);
                break;
            case MouseAction.RightUp:
                await _simulator.MouseUp(MouseButton.Right);
                break;
            case MouseAction.MiddleDown:
                await _simulator.MouseDown(MouseButton.Middle);
                break;
            case MouseAction.MiddleUp:
                await _simulator.MouseUp(MouseButton.Middle);
                break;
            case MouseAction.Wheel:
                await _simulator.MouseWheel(me.WheelDelta);
                break;
        }

        return PlaybackResult.Continue();
    }
}
