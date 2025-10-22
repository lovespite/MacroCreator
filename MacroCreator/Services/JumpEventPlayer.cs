// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;

namespace MacroCreator.Services;

/// <summary>
/// 处理跳转事件的播放器
/// </summary>
public class JumpEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        if (context.CurrentEvent is JumpEvent jumpEvent)
        {
            int targetIndex = context.FindEventIndexByName(jumpEvent.TargetEventName);

            if (targetIndex < 0)
            {
                throw new EventFlowControlException($"无效的跳转目标: '{jumpEvent.TargetEventName}'", context.CurrentEvent, context.CurrentEventIndex);
            }

            // 返回跳转结果
            return Task.FromResult(PlaybackResult.Jump(targetIndex));
        }

        return Task.FromResult(PlaybackResult.Continue());
    }
}