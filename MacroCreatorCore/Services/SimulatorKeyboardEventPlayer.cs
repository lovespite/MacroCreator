using MacroCreator.Models;
using MacroCreator.Models.Events;

namespace MacroCreator.Services;

public class SimulatorKeyboardEventPlayer(ISimulator _simulator) : IEventPlayer
{
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