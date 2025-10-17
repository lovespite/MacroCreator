using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MacroCreator.Services;

/// <summary>
/// 高精度计时器服务，提供精确的延迟功能
/// </summary>
public class HighPrecisionTimer
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);

    [DllImport("winmm.dll", SetLastError = true)]
    private static extern int timeBeginPeriod(int uPeriod);

    [DllImport("winmm.dll", SetLastError = true)]
    private static extern int timeEndPeriod(int uPeriod);

    private readonly long _frequency;
    private bool _highResolutionEnabled = false;

    public HighPrecisionTimer()
    {
        QueryPerformanceFrequency(out _frequency);
        EnableHighResolution();
    }

    /// <summary>
    /// 启用高分辨率定时器（提高系统定时器精度到1ms）
    /// </summary>
    private void EnableHighResolution()
    {
        if (!_highResolutionEnabled)
        {
            timeBeginPeriod(1);
            _highResolutionEnabled = true;
        }
    }

    /// <summary>
    /// 禁用高分辨率定时器
    /// </summary>
    public void DisableHighResolution()
    {
        if (_highResolutionEnabled)
        {
            timeEndPeriod(1);
            _highResolutionEnabled = false;
        }
    }

    /// <summary>
    /// 获取当前高精度时间戳（微秒）
    /// </summary>
    public long GetMicroseconds()
    {
        QueryPerformanceCounter(out long counter);
        return (counter * 1000000) / _frequency;
    }

    /// <summary>
    /// 获取当前高精度时间戳（毫秒，带小数）
    /// </summary>
    public double GetPreciseMilliseconds()
    {
        QueryPerformanceCounter(out long counter);
        return (double)(counter * 1000) / _frequency;
    }

    /// <summary>
    /// 精确延迟指定的毫秒数
    /// </summary>
    public async Task DelayAsync(double milliseconds, CancellationToken cancellationToken = default)
    {
        if (milliseconds <= 0) return;

        var startTime = GetPreciseMilliseconds();
        var targetTime = startTime + milliseconds;

        // 对于较长的延迟（>10ms），使用Task.Delay处理大部分时间
        if (milliseconds > 10)
        {
            var roughDelay = (int)(milliseconds - 2); // 留2ms用于精确等待
            if (roughDelay > 0)
            {
                await Task.Delay(roughDelay, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }
        }

        // 使用忙等待处理剩余的精确时间
        while (GetPreciseMilliseconds() < targetTime)
        {
            if (cancellationToken.IsCancellationRequested) return;
            
            // 短暂让出CPU，避免100%占用
            if (targetTime - GetPreciseMilliseconds() > 0.5)
            {
                await Task.Yield();
            }
        }
    }

    /// <summary>
    /// 等待到指定的目标时间点
    /// </summary>
    public async Task WaitUntilAsync(double targetTimeMs, CancellationToken cancellationToken = default)
    {
        var currentTime = GetPreciseMilliseconds();
        var delay = targetTimeMs - currentTime;
        
        if (delay > 0)
        {
            await DelayAsync(delay, cancellationToken);
        }
    }

    public void Dispose()
    {
        DisableHighResolution();
    }
}