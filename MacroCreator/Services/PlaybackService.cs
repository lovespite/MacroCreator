// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// 负责回放事件序列，使用高精度计时器确保准确的延迟
/// </summary>
public class PlaybackService : IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Dictionary<Type, IEventPlayer> _players;
    private readonly HighPrecisionTimer _timer;
    private readonly PlaybackPerformanceMonitor _performanceMonitor;

    public PlaybackService(Dictionary<Type, IEventPlayer> players)
    {
        _players = players;
        _timer = new HighPrecisionTimer();
        _performanceMonitor = new PlaybackPerformanceMonitor();
    }

    /// <summary>
    /// 性能监控器，用于分析延迟精度
    /// </summary>
    public PlaybackPerformanceMonitor PerformanceMonitor => _performanceMonitor;

    public async Task Play(List<RecordedEvent> events, Func<string, Task> loadAndPlayNewFileCallback)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var context = new PlaybackContext(_cancellationTokenSource.Token, loadAndPlayNewFileCallback);

        var startTime = _timer.GetPreciseMilliseconds();
        var scheduledTime = startTime;

        int currentIndex = 0;
        while (currentIndex < events.Count && !context.CancellationToken.IsCancellationRequested)
        {
            var ev = events[currentIndex];

            // 计算下一个事件应该执行的时间点
            scheduledTime += ev.TimeSinceLastEvent;
            
            // 等待到预定的执行时间
            await _timer.WaitUntilAsync(scheduledTime, context.CancellationToken);

            if (context.CancellationToken.IsCancellationRequested) break;

            if (_players.TryGetValue(ev.GetType(), out var player))
            {
                try
                {
                    // 记录实际执行时间，用于性能监控
                    var actualExecutionTime = _timer.GetPreciseMilliseconds();
                    _performanceMonitor.RecordMeasurement(scheduledTime, actualExecutionTime, ev.GetType().Name);
                    
                    await player.ExecuteAsync(ev, context);
                    
                    // 检查是否有跳转请求
                    if (context.HasJumpTarget)
                    {
                        var targetIndex = context.JumpTargetIndex;
                        context.ClearJumpTarget();
                        
                        // 验证跳转目标是否有效
                        if (targetIndex >= 0 && targetIndex < events.Count)
                        {
                            currentIndex = targetIndex;
                            // 重置时间基准，从跳转目标开始计算时间
                            scheduledTime = _timer.GetPreciseMilliseconds();
                            continue;
                        }
                        else
                        {
                            // 无效的跳转目标，终止播放
                            return;
                        }
                    }
                    
                    // 如果是DelayEvent，需要额外处理其内部延迟
                    if (ev is DelayEvent delayEvent)
                    {
                        scheduledTime += delayEvent.DelayMilliseconds;
                    }
                }
                catch (SequenceJumpException)
                {
                    // 检查是否是跳转到新文件
                    if (context.LoadAndPlayNewFileCallback != null && !context.HasJumpTarget)
                    {
                        // 这是文件跳转，终止当前循环
                        return;
                    }
                    
                    // 这是序列内跳转，已在上面处理
                    if (context.HasJumpTarget)
                    {
                        var targetIndex = context.JumpTargetIndex;
                        context.ClearJumpTarget();
                        
                        if (targetIndex >= 0 && targetIndex < events.Count)
                        {
                            currentIndex = targetIndex;
                            scheduledTime = _timer.GetPreciseMilliseconds();
                            continue;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            
            currentIndex++;
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }

    public void Dispose()
    {
        Stop();
        _timer?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
