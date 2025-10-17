// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Services;

namespace MacroCreator;

public class DelayEventPlayer : IEventPlayer
{
    public async Task ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        var de = (DelayEvent)ev;
        await Task.Delay(de.DelayMilliseconds, context.CancellationToken);
    }
} 

