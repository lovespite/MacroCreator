namespace MacroCreator.Services;

public interface ISystemTimer : IDisposable
{
    double GetPreciseMilliseconds();
    Task DelayAsync(double milliseconds, CancellationToken cancellationToken = default);
    Task WaitUntilAsync(double targetTimeMs, CancellationToken cancellationToken = default);
}