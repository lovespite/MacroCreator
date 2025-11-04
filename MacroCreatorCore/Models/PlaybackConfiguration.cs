namespace MacroCreator.Models;

/// <summary>
/// 回放配置类，用于调整延迟精度和性能参数
/// </summary>
public class PlaybackConfiguration
{
    /// <summary>
    /// 是否启用高精度延迟模式
    /// </summary>
    public bool HighPrecisionMode { get; set; } = true;

    /// <summary>
    /// 最小延迟阈值（毫秒），低于此值的延迟将被跳过
    /// </summary>
    public double MinimumDelayThreshold { get; set; } = 0.1;

    /// <summary>
    /// 最大累积误差阈值（毫秒），超过此值时将进行时间校正
    /// </summary>
    public double MaxAccumulatedErrorThreshold { get; set; } = 5.0;

    /// <summary>
    /// 长延迟阈值（毫秒），超过此值时使用 Task.Delay 处理大部分时间
    /// </summary>
    public double LongDelayThreshold { get; set; } = 10.0;

    /// <summary>
    /// 精确等待预留时间（毫秒），用于最后的精确调整
    /// </summary>
    public double PreciseWaitReserve { get; set; } = 2.0;

    /// <summary>
    /// 是否启用性能监控
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = false;

    /// <summary>
    /// 是否启用时间校正（补偿累积误差）
    /// </summary>
    public bool EnableTimeCorrection { get; set; } = true;

    /// <summary>
    /// CPU yield 阈值（毫秒），剩余等待时间大于此值时才让出CPU
    /// </summary>
    public double CpuYieldThreshold { get; set; } = 0.5;

    /// <summary>
    /// 默认配置
    /// </summary>
    public static PlaybackConfiguration Default => new PlaybackConfiguration();

    /// <summary>
    /// 高性能配置（更激进的精度设置）
    /// </summary>
    public static PlaybackConfiguration HighPerformance => new PlaybackConfiguration
    {
        HighPrecisionMode = true,
        MinimumDelayThreshold = 0.05,
        MaxAccumulatedErrorThreshold = 2.0,
        LongDelayThreshold = 5.0,
        PreciseWaitReserve = 1.0,
        EnablePerformanceMonitoring = true,
        EnableTimeCorrection = true,
        CpuYieldThreshold = 0.2
    };

    /// <summary>
    /// 节能配置（较低的CPU使用率）
    /// </summary>
    public static PlaybackConfiguration PowerSaving => new PlaybackConfiguration
    {
        HighPrecisionMode = false,
        MinimumDelayThreshold = 1.0,
        MaxAccumulatedErrorThreshold = 10.0,
        LongDelayThreshold = 20.0,
        PreciseWaitReserve = 5.0,
        EnablePerformanceMonitoring = false,
        EnableTimeCorrection = false,
        CpuYieldThreshold = 2.0
    };
}