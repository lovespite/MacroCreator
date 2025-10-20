// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 无条件跳转事件 - 跳转到指定的事件索引或名称
/// </summary>
public class JumpEvent : RecordedEvent
{
    /// <summary>
    /// 目标事件的名称 - 优先使用名称跳转
    /// </summary>
    public string TargetEventName { get; set; } = string.Empty;

    public override string GetDescription()
    {
        return $"跳转事件: {TargetEventName}";
    }
}