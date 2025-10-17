// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

public class PlaybackContext
{
    public CancellationToken CancellationToken { get; }
    public Func<string, Task>? LoadAndPlayNewFileCallback { get; }
    
    /// <summary>
    /// 跳转目标索引，-1表示无跳转
    /// </summary>
    public int JumpTargetIndex { get; private set; } = -1;
    
    /// <summary>
    /// 是否有待处理的跳转
    /// </summary>
    public bool HasJumpTarget => JumpTargetIndex >= 0;
    
    public PlaybackContext(CancellationToken token, Func<string, Task>? callback)
    {
        CancellationToken = token;
        LoadAndPlayNewFileCallback = callback;
    }
    
    /// <summary>
    /// 设置跳转目标
    /// </summary>
    /// <param name="targetIndex">目标事件索引</param>
    public void SetJumpTarget(int targetIndex)
    {
        JumpTargetIndex = targetIndex;
    }
    
    /// <summary>
    /// 清除跳转目标
    /// </summary>
    public void ClearJumpTarget()
    {
        JumpTargetIndex = -1;
    }
}
