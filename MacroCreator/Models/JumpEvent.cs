// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 无条件跳转事件 - 跳转到指定的事件索引或名称
/// </summary>
public class JumpEvent : RecordedEvent
{
    /// <summary>
    /// 目标事件的索引（从0开始）- 用于向后兼容或当未指定名称时
    /// </summary>
    [Obsolete("使用 TargetEventName 代替")]
    public int TargetIndex { get; set; }

    /// <summary>
    /// 目标事件的名称 - 优先使用名称跳转
    /// </summary>
    public string TargetEventName { get; set; } = string.Empty;

    /// <summary>
    /// 可选的标签名称，用于更直观的跳转目标标识（已弃用，使用 TargetEventName）
    /// </summary>
    [Obsolete("使用 TargetEventName 代替")]
    public string? Label { get; set; }

    public override string GetDescription()
    {
        return $"跳转事件: {TargetEventName}";
    }
}