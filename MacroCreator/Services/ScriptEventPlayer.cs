// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;

namespace MacroCreator.Services;

public class ScriptEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        var scriptEvent = (ScriptEvent)context.CurrentEvent;
        var scripts = scriptEvent.ScriptLines;

        int lIndex = -1;
        try
        {
            for (lIndex = 0; lIndex < scripts.Length; lIndex++)
            {
                var script = scripts[lIndex];

                if (string.IsNullOrWhiteSpace(script))
                    continue;
                if (script.StartsWith("//"))
                    continue;

                _ = context.Execute(script);
            }
        }
        catch (Exception ex)
        {
            throw new EventPlayerException($"脚本执行出错在 {lIndex}: {ex.Message}", scriptEvent, context.CurrentEventIndex);
        }

        return Task.FromResult(PlaybackResult.Continue());
    }
}