// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;

namespace MacroCreator.Services;

public class ScriptEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        var scriptEvent = (ScriptEvent)context.CurrentEvent;
        var script = scriptEvent.Expression;

        _ = context.Execute(script);

        return Task.FromResult(PlaybackResult.Continue());
    }
}