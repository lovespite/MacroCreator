using MacroCreator.Models;

namespace MacroCreator.Services;

public interface IEventPlayer
{
    Task<PlaybackResult> ExecuteAsync(PlaybackContext context);
}

