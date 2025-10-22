// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;

namespace MacroCreator.Services;

/// <summary>
/// 负责回放事件序列，使用高精度计时器确保准确的延迟
/// </summary>
public class PlaybackService(Dictionary<Type, IEventPlayer> players) : IDisposable
{
    private readonly Dictionary<Type, IEventPlayer> _players = players;
    private readonly HighPrecisionTimer _timer = new();
    private readonly PlaybackPerformanceMonitor _performanceMonitor = new();

    private PlaybackContext? _context = null;

    /// <summary>
    /// 性能监控器，用于分析延迟精度
    /// </summary>
    public PlaybackPerformanceMonitor PerformanceMonitor => _performanceMonitor;

    public async Task Play(IReadOnlyList<MacroEvent> events, CallExternalFileDelegate? loadAndPlayNewFileCallback)
    {
        using var context = _context = new PlaybackContext(events, loadAndPlayNewFileCallback);

        var startTime = _timer.GetPreciseMilliseconds();
        var scheduledTime = startTime;

        while (context.CurrentEventIndex < events.Count && !context.CancellationToken.IsCancellationRequested)
        {
            var ev = context.CurrentEvent;

            // 计算下一个事件应该执行的时间点
            scheduledTime += ev.TimeSinceLastEvent;

            // 等待到预定的执行时间
            await _timer.WaitUntilAsync(scheduledTime, context.CancellationToken);

            if (context.CancellationToken.IsCancellationRequested) break;

            if (!_players.TryGetValue(ev.GetType(), out var player))
                throw new EventPlayerException($"无法处理 {ev.TypeName} 事件", ev, context.CurrentEventIndex);

            // 记录实际执行时间，用于性能监控
            var actualExecutionTime = _timer.GetPreciseMilliseconds();
            _performanceMonitor.RecordMeasurement(scheduledTime, actualExecutionTime, ev.TypeName);

            // 执行事件并获取结果
            var result = await player.ExecuteAsync(context);

            // 根据结果处理控制流
            switch (result.Control)
            {
                case PlaybackControl.Continue:
                    // 如果是DelayEvent，需要额外处理其内部延迟
                    if (ev is DelayEvent delayEvent)
                    {
                        scheduledTime += delayEvent.DelayMilliseconds;
                    }
                    // 移动到下一个事件
                    if (!context.MoveNext()) return; // 序列已经到达末尾
                    break;

                case PlaybackControl.Jump:
                    // 验证跳转目标是否有效
                    if (result.TargetIndex >= 0 && result.TargetIndex < events.Count)
                    {
                        context.MoveTo(result.TargetIndex);
                        // 重置时间基准，从跳转目标开始计算时间
                        scheduledTime = _timer.GetPreciseMilliseconds();
                    }
                    else
                    {
                        // 无效的跳转目标，终止播放
                        throw new EventFlowControlException($"无效的跳转目标", ev, context.CurrentEventIndex);
                    }
                    break;

                case PlaybackControl.JumpToFile:
                    if (result.FilePath is null || loadAndPlayNewFileCallback is null)
                    {
                        throw new EventFlowControlException($"无法处理外部文件", ev, context.CurrentEventIndex);
                    }
                    await loadAndPlayNewFileCallback(result.FilePath);
                    // 终止当前序列播放
                    return;

                case PlaybackControl.Break: return; // 终止播放 

                default:
                    throw new EventPlayerException($"未知指令: {result.Control}", ev, context.CurrentEventIndex);
            }
        }
    }

    public void Stop()
    {
        _context?.Abort();
        _context = null;
    }

    public void Dispose()
    {
        Stop();

        _timer?.Dispose();

        GC.SuppressFinalize(this);
    }
}