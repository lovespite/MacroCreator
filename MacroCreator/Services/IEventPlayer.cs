using MacroCreator.Models;

namespace MacroCreator.Services;

public interface IEventPlayer
{
    Task<PlaybackResult> ExecuteAsync(RecordedEvent ev, PlaybackContext context);
}

