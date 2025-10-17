// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

public class PlaybackContext
{
    public CancellationToken CancellationToken { get; }
    public Func<string, Task>? LoadAndPlayNewFileCallback { get; }
    public PlaybackContext(CancellationToken token, Func<string, Task>? callback)
    {
        CancellationToken = token;
        LoadAndPlayNewFileCallback = callback;
    }
}
