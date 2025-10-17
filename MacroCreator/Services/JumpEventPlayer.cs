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
            // 返回跳转结果
            return Task.FromResult(PlaybackResult.Jump(jumpEvent.TargetIndex));
        }
        
        return Task.FromResult(PlaybackResult.Continue());
    }
}