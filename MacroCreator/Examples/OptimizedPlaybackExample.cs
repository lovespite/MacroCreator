using MacroCreator.Models;
using MacroCreator.Services;

namespace MacroCreator.Examples;

/// <summary>
/// 展示如何使用优化后的延迟架构
/// </summary>
public static class OptimizedPlaybackExample
{
    /// <summary>
    /// 创建优化的回放服务实例
    /// </summary>
    public static PlaybackService CreateOptimizedPlaybackService()
    {
        var players = new Dictionary<Type, IEventPlayer>
        {
            { typeof(MouseEvent), new MouseEventPlayer() },
            { typeof(KeyboardEvent), new KeyboardEventPlayer() },
            { typeof(DelayEvent), new DelayEventPlayer() },
            { typeof(PixelConditionEvent), new PixelConditionEventPlayer() },
            { typeof(JumpEvent), new JumpEventPlayer() },
            { typeof(ConditionalJumpEvent), new ConditionalJumpEventPlayer() }
        };

        var playbackService = new PlaybackService(players);
        
        // 启用性能监控（可选）
        playbackService.PerformanceMonitor.IsEnabled = true;
        
        return playbackService;
    }

    /// <summary>
    /// 演示高精度回放
    /// </summary>
    public static async Task DemonstrateHighPrecisionPlayback(List<RecordedEvent> events)
    {
        using var playbackService = CreateOptimizedPlaybackService();
        
        // 启用性能监控
        playbackService.PerformanceMonitor.IsEnabled = true;
        
        Console.WriteLine("开始高精度回放...");
        
        await playbackService.Play(events, async (filename) => {
            // 处理序列跳转
            Console.WriteLine($"跳转到新文件: {filename}");
        });
        
        // 获取性能统计
        var stats = playbackService.PerformanceMonitor.GetStatistics();
        Console.WriteLine($"回放完成!");
        Console.WriteLine($"平均延迟偏差: {stats.AverageDeviation:F3}ms");
        Console.WriteLine($"最大延迟偏差: {stats.MaxDeviation:F3}ms");
        Console.WriteLine($"延迟准确度: {stats.AccuracyPercentage:F2}%");
        
        // 生成详细报告
        var report = playbackService.PerformanceMonitor.GenerateReport();
        Console.WriteLine(report);
    }

    /// <summary>
    /// 演示创建高精度录制服务
    /// </summary>
    public static RecordingService CreateOptimizedRecordingService()
    {
        var recordingService = new RecordingService();
        
        recordingService.OnEventRecorded += (ev) => {
            Console.WriteLine($"录制事件: {ev.GetDescription()}, 时间间隔: {ev.TimeSinceLastEvent:F3}ms");
        };
        
        return recordingService;
    }

    /// <summary>
    /// 测试高精度计时器
    /// </summary>
    public static async Task TestHighPrecisionTimer()
    {
        var timer = new HighPrecisionTimer();
        
        Console.WriteLine("测试高精度延迟...");
        
        var testDelays = new[] { 0.5, 1.0, 1.5, 5.0, 10.0, 50.0 };
        
        foreach (var delayMs in testDelays)
        {
            var startTime = timer.GetPreciseMilliseconds();
            await timer.DelayAsync(delayMs);
            var endTime = timer.GetPreciseMilliseconds();
            var actualDelay = endTime - startTime;
            var error = actualDelay - delayMs;
            
            Console.WriteLine($"预期延迟: {delayMs}ms, 实际延迟: {actualDelay:F3}ms, 误差: {error:F3}ms");
        }
    }
}