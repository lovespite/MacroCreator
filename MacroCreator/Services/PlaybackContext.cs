// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

/// <summary>
/// 播放上下文，包含播放过程中需要的共享状态
/// </summary>
public class PlaybackContext
{
    /// <summary>
    /// 取消令牌，用于停止播放
    /// </summary>
    public CancellationToken CancellationToken { get; }
    
    /// <summary>
    /// 加载并播放新文件的回调函数
    /// </summary>
    public Func<string, Task>? LoadAndPlayNewFileCallback { get; }
    
    public PlaybackContext(CancellationToken token, Func<string, Task>? callback)
    {
        CancellationToken = token;
        LoadAndPlayNewFileCallback = callback;
    }
}
