// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// Break事件播放器 - 用于终止宏执行
/// </summary>
public class BreakEventPlayer : IEventPlayer
{
    public Task ExecuteAsync(RecordedEvent recordedEvent, PlaybackContext context)
    {
        if (recordedEvent is not BreakEvent)
        {
            throw new ArgumentException("事件类型必须是 BreakEvent", nameof(recordedEvent));
        }
        else
        {
            // 抛出特定的异常来中断流程
            throw new SequenceJumpException(-1)
            {
                IsBreak = true
            };
        }
    }
}
