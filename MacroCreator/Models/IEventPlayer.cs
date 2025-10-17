using MacroCreator.Services;

namespace MacroCreator.Models;

public interface IEventPlayer
{
    Task ExecuteAsync(RecordedEvent ev, PlaybackContext context);
}

