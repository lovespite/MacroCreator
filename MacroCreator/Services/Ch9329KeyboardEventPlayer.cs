using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Services.CH9329;

namespace MacroCreator.Services;

/// <summary>
/// 使用 CH9329 InputSimulator 回放键盘事件的播放器
/// </summary>
public class Ch9329KeyboardEventPlayer : IEventPlayer
{
    private readonly InputSimulator _simulator;

    public Ch9329KeyboardEventPlayer(InputSimulator simulator)
    {
        _simulator = simulator;
    }

    public async Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        if (context.CurrentEvent is not KeyboardEvent ke)
        {
            return PlaybackResult.Continue();
        }

        switch (ke.Action)
        {
            case KeyboardAction.KeyDown:
                await _simulator.KeyDown(ke.Key);
                break;
            case KeyboardAction.KeyUp:
                await _simulator.KeyUp(ke.Key);
                break;
                // KeyPress 在原始 KeyboardEventPlayer 中未处理，这里也保持一致
        }

        return PlaybackResult.Continue();
    }
}