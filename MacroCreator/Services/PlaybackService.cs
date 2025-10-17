// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// 负责回放事件序列，使用高精度计时器确保准确的延迟
/// </summary>
public class PlaybackService(Dictionary<Type, IEventPlayer> players) : IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Dictionary<Type, IEventPlayer> _players = players;
    private readonly HighPrecisionTimer _timer = new();
    private readonly PlaybackPerformanceMonitor _performanceMonitor = new();

    /// <summary>
    /// 性能监控器，用于分析延迟精度
    /// </summary>
    public PlaybackPerformanceMonitor PerformanceMonitor => _performanceMonitor;

    public async Task Play(List<RecordedEvent> events, Func<string, Task>? loadAndPlayNewFileCallback)
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
                // 记录实际执行时间，用于性能监控
                var actualExecutionTime = _timer.GetPreciseMilliseconds();
                _performanceMonitor.RecordMeasurement(scheduledTime, actualExecutionTime, ev.GetType().Name);

                // 执行事件并获取结果
                var result = await player.ExecuteAsync(ev, context);

                // 根据结果处理控制流
                switch (result.Control)
                {
                    case PlaybackControl.Continue:
                        // 如果是DelayEvent，需要额外处理其内部延迟
                        if (ev is DelayEvent delayEvent)
                        {
                            scheduledTime += delayEvent.DelayMilliseconds;
                        }
                        currentIndex++;
                        break;

                    case PlaybackControl.Jump:
                        // 验证跳转目标是否有效
                        if (result.TargetIndex >= 0 && result.TargetIndex < events.Count)
                        {
                            currentIndex = result.TargetIndex;
                            // 重置时间基准，从跳转目标开始计算时间
                            scheduledTime = _timer.GetPreciseMilliseconds();
                        }
                        else
                        {
                            // 无效的跳转目标，终止播放
                            return;
                        }
                        break;

                    case PlaybackControl.Break:
                        // 终止播放
                        return;

                    case PlaybackControl.JumpToFile:
                        // 跳转到外部文件
                        if (!string.IsNullOrEmpty(result.FilePath) && loadAndPlayNewFileCallback != null)
                        {
                            await loadAndPlayNewFileCallback(result.FilePath);
                        }
                        // 终止当前序列播放
                        return;

                    default:
                        currentIndex++;
                        break;
                }
            }
            else
            {
                currentIndex++;
            }
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

        GC.SuppressFinalize(this);
    }
}
