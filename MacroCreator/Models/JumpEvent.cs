// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 无条件跳转事件 - 跳转到指定的事件索引
/// </summary>
public class JumpEvent : RecordedEvent
{
    /// <summary>
    /// 目标事件的索引（从0开始）
    /// </summary>
    public int TargetIndex { get; set; }

    /// <summary>
    /// 可选的标签名称，用于更直观的跳转目标标识
    /// </summary>
    public string? Label { get; set; }

    public override string GetDescription()
    {
        if (!string.IsNullOrEmpty(Label))
        {
            return $"跳转到事件 #{TargetIndex + 1} (标签: {Label})";
        }
        return $"跳转到事件 #{TargetIndex + 1}";
    }
}