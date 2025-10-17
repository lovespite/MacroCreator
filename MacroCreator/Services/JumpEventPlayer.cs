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
            int targetIndex = jumpEvent.TargetIndex;

            // 如果指定了目标事件名称，优先使用名称查找
            if (!string.IsNullOrWhiteSpace(jumpEvent.TargetEventName))
            {
                int foundIndex = context.FindEventIndexByName(jumpEvent.TargetEventName);
                if (foundIndex >= 0)
                {
                    targetIndex = foundIndex;
                }
                else
                {
                    // 无法跳转到匿名事件或未找到的事件，继续执行下一个事件
                    return Task.FromResult(PlaybackResult.Continue());
                }
            }

            // 返回跳转结果
            return Task.FromResult(PlaybackResult.Jump(targetIndex));
        }
        
        return Task.FromResult(PlaybackResult.Continue());
    }
}