// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// 处理跳转事件的播放器
/// </summary>
public class JumpEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        if (ev is JumpEvent jumpEvent)
        {
            int targetIndex = context.FindEventIndexByName(jumpEvent.TargetEventName);

            if (targetIndex < 0)
            {
                throw new InvalidOperationException($"无法跳转到未命名或不存在的事件: '{jumpEvent.TargetEventName}'");
            }

            // 返回跳转结果
            return Task.FromResult(PlaybackResult.Jump(targetIndex));
        }

        return Task.FromResult(PlaybackResult.Continue());
    }
}