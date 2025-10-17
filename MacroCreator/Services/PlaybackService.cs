// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// 负责回放事件序列
/// </summary>
public class PlaybackService
{
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly Dictionary<Type, IEventPlayer> _players;

    public PlaybackService(Dictionary<Type, IEventPlayer> players)
    {
        _players = players;
    }

    public async Task Play(List<RecordedEvent> events, Func<string, Task> loadAndPlayNewFileCallback)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var context = new PlaybackContext(_cancellationTokenSource.Token, loadAndPlayNewFileCallback);

        foreach (var ev in events)
        {
            if (context.CancellationToken.IsCancellationRequested) break;

            await Task.Delay((int)ev.TimeSinceLastEvent, context.CancellationToken);

            if (context.CancellationToken.IsCancellationRequested) break;

            if (_players.TryGetValue(ev.GetType(), out var player))
            {
                try
                {
                    await player.ExecuteAsync(ev, context);
                }
                catch (SequenceJumpException)
                {
                    // 这是一个控制流异常，表示已跳转到新序列，应终止当前循环
                    return;
                }
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
    }
}
