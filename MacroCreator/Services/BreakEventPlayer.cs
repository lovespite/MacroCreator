// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;

namespace MacroCreator.Services;

/// <summary>
/// Break事件播放器 - 用于终止宏执行
/// </summary>
public class BreakEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        if (context.CurrentEvent is not BreakEvent)
        {
            throw new ArgumentException("事件类型必须是 BreakEvent", context.CurrentEvent.TypeName);
        }

        // 返回中断结果
        return Task.FromResult(PlaybackResult.Break());
    }
}
