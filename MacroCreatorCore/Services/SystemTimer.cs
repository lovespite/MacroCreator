namespace MacroCreator.Services;

public class SystemTimer : ISystemTimer
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public Task DelayAsync(double milliseconds, CancellationToken cancellationToken = default)
    {
        return Task.Delay(TimeSpan.FromMilliseconds(milliseconds), cancellationToken);
    }

    public double GetPreciseMilliseconds()
    {
        return (DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds;
    }

    public Task WaitUntilAsync(double targetTimeMs, CancellationToken cancellationToken = default)
    {
        var delay = targetTimeMs - GetPreciseMilliseconds();
        if (delay <= 0)
            return Task.CompletedTask;
        return Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken);
    }
}
