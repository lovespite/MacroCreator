using System.Text;

namespace MacroCreator.Services;

/// <summary>
/// 性能监控器，用于分析和调试延迟精度
/// </summary>
public class PlaybackPerformanceMonitor
{
    private readonly List<TimingMeasurement> _measurements = new();
    private bool _isEnabled;

    public bool IsEnabled 
    { 
        get => _isEnabled; 
        set => _isEnabled = value; 
    }

    /// <summary>
    /// 记录一个时间测量
    /// </summary>
    public void RecordMeasurement(double scheduledTime, double actualTime, string eventType)
    {
        if (!_isEnabled) return;

        var measurement = new TimingMeasurement
        {
            ScheduledTime = scheduledTime,
            ActualTime = actualTime,
            Deviation = actualTime - scheduledTime,
            EventType = eventType,
            Timestamp = DateTime.UtcNow
        };

        _measurements.Add(measurement);
    }

    /// <summary>
    /// 获取性能统计信息
    /// </summary>
    public PlaybackStatistics GetStatistics()
    {
        if (_measurements.Count == 0)
        {
            return new PlaybackStatistics();
        }

        var deviations = _measurements.Select(m => m.Deviation).ToArray();
        
        return new PlaybackStatistics
        {
            TotalMeasurements = _measurements.Count,
            AverageDeviation = deviations.Average(),
            MaxDeviation = deviations.Max(),
            MinDeviation = deviations.Min(),
            StandardDeviation = CalculateStandardDeviation(deviations),
            AccuracyPercentage = CalculateAccuracyPercentage()
        };
    }

    /// <summary>
    /// 生成详细的性能报告
    /// </summary>
    public string GenerateReport()
    {
        var stats = GetStatistics();
        var sb = new StringBuilder();

        sb.AppendLine("=== 回放性能报告 ===");
        sb.AppendLine($"总测量次数: {stats.TotalMeasurements}");
        sb.AppendLine($"平均偏差: {stats.AverageDeviation:F3}ms");
        sb.AppendLine($"最大偏差: {stats.MaxDeviation:F3}ms");
        sb.AppendLine($"最小偏差: {stats.MinDeviation:F3}ms");
        sb.AppendLine($"标准差: {stats.StandardDeviation:F3}ms");
        sb.AppendLine($"准确度: {stats.AccuracyPercentage:F2}%");
        sb.AppendLine();

        // 按事件类型分组统计
        var groupedStats = _measurements
            .GroupBy(m => m.EventType)
            .Select(g => new
            {
                EventType = g.Key,
                Count = g.Count(),
                AvgDeviation = g.Average(m => m.Deviation),
                MaxDeviation = g.Max(m => m.Deviation)
            })
            .OrderBy(x => x.EventType);

        sb.AppendLine("=== 按事件类型统计 ===");
        foreach (var stat in groupedStats)
        {
            sb.AppendLine($"{stat.EventType}: 次数={stat.Count}, 平均偏差={stat.AvgDeviation:F3}ms, 最大偏差={stat.MaxDeviation:F3}ms");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 清除所有测量数据
    /// </summary>
    public void Clear()
    {
        _measurements.Clear();
    }

    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length == 0) return 0;

        var mean = values.Average();
        var sumOfSquares = values.Sum(val => (val - mean) * (val - mean));
        return Math.Sqrt(sumOfSquares / values.Length);
    }

    private double CalculateAccuracyPercentage()
    {
        if (_measurements.Count == 0) return 0;

        // 定义"准确"为偏差小于1ms的事件
        var accurateCount = _measurements.Count(m => Math.Abs(m.Deviation) < 1.0);
        return (double)accurateCount / _measurements.Count * 100;
    }
}

/// <summary>
/// 时间测量记录
/// </summary>
public class TimingMeasurement
{
    public double ScheduledTime { get; set; }
    public double ActualTime { get; set; }
    public double Deviation { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 回放性能统计数据
/// </summary>
public class PlaybackStatistics
{
    public int TotalMeasurements { get; set; }
    public double AverageDeviation { get; set; }
    public double MaxDeviation { get; set; }
    public double MinDeviation { get; set; }
    public double StandardDeviation { get; set; }
    public double AccuracyPercentage { get; set; }
}