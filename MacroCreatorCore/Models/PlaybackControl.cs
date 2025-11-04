namespace MacroCreator.Models;

/// <summary>
/// 定义播放控制流类型
/// </summary>
public enum PlaybackControl
{
    /// <summary>
    /// 继续执行下一个事件
    /// </summary>
    Continue,
    
    /// <summary>
    /// 跳转到指定索引
    /// </summary>
    Jump,
    
    /// <summary>
    /// 中断并终止播放
    /// </summary>
    Break,
    
    /// <summary>
    /// 跳转到外部文件
    /// </summary>
    JumpToFile
}
