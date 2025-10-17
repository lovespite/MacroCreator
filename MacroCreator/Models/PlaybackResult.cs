namespace MacroCreator.Models;

/// <summary>
/// 封装事件播放器的执行结果
/// </summary>
public class PlaybackResult
{
    /// <summary>
    /// 控制流指令
    /// </summary>
    public PlaybackControl Control { get; init; }
    
    /// <summary>
    /// 跳转目标索引（仅当 Control 为 Jump 时有效）
    /// </summary>
    public int TargetIndex { get; init; }
    
    /// <summary>
    /// 外部文件路径（仅当 Control 为 JumpToFile 时有效）
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// 创建一个"继续"结果
    /// </summary>
    public static PlaybackResult Continue() => new() { Control = PlaybackControl.Continue };
    
    /// <summary>
    /// 创建一个"跳转"结果
    /// </summary>
    public static PlaybackResult Jump(int targetIndex) => new() 
    { 
        Control = PlaybackControl.Jump, 
        TargetIndex = targetIndex 
    };
    
    /// <summary>
    /// 创建一个"中断"结果
    /// </summary>
    public static PlaybackResult Break() => new() { Control = PlaybackControl.Break };
    
    /// <summary>
    /// 创建一个"跳转到文件"结果
    /// </summary>
    public static PlaybackResult JumpToFile(string filePath) => new() 
    { 
        Control = PlaybackControl.JumpToFile, 
        FilePath = filePath 
    };
}
