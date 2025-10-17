using MacroCreator.Models;

namespace MacroCreator.Services;

public interface IEventPlayer
{
    Task ExecuteAsync(RecordedEvent ev, PlaybackContext context);
}

