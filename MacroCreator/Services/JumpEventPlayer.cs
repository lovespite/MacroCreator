// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// 处理跳转事件的播放器
/// </summary>
public class JumpEventPlayer : IEventPlayer
{
    public Task ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        if (ev is JumpEvent jumpEvent)
        {
            // 设置跳转目标并抛出跳转异常来中断当前播放循环
            context.SetJumpTarget(jumpEvent.TargetIndex);
            throw new SequenceJumpException();
        }
        
        return Task.CompletedTask;
    }
}