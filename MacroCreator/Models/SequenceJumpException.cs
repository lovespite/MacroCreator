// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 用于在跳转或中断后中断当前播放循环的自定义异常
/// </summary>
public class SequenceJumpException : OperationCanceledException 
{ 
    /// <summary>
    /// 目标跳转索引（-1表示中断执行）
    /// </summary>
    public int TargetIndex { get; }

    /// <summary>
    /// 是否为Break中断操作
    /// </summary>
    public bool IsBreak { get; set; }

    public SequenceJumpException(int targetIndex)
    {
        TargetIndex = targetIndex;
    }
}


