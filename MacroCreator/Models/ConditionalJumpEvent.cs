// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 条件跳转事件 - 基于指定条件决定是否跳转
/// </summary>
public class ConditionalJumpEvent : RecordedEvent
{
    /// <summary>
    /// 条件类型
    /// </summary>
    public ConditionType ConditionType { get; set; }

    /// <summary>
    /// 像素检查的X坐标（当ConditionType为PixelColor时使用）
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// 像素检查的Y坐标（当ConditionType为PixelColor时使用）
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// 预期的颜色值（当ConditionType为PixelColor时使用）
    /// </summary>
    public string? ExpectedColorHex { get; set; }

    /// <summary>
    /// 自定义条件表达式（当ConditionType为Custom时使用）
    /// </summary>
    public string? CustomCondition { get; set; }

    /// <summary>
    /// 条件为真时跳转的目标索引
    /// </summary>
    public int TrueTargetIndex { get; set; }

    /// <summary>
    /// 条件为假时跳转的目标索引（-1表示继续执行下一个事件）
    /// </summary>
    public int FalseTargetIndex { get; set; } = -1;

    /// <summary>
    /// 条件为真时的标签
    /// </summary>
    public string? TrueLabel { get; set; }

    /// <summary>
    /// 条件为假时的标签
    /// </summary>
    public string? FalseLabel { get; set; }

    public override string GetDescription()
    {
        var conditionDesc = ConditionType switch
        {
            ConditionType.PixelColor => $"检查点({X},{Y})颜色是否为{ExpectedColorHex}",
            ConditionType.Custom => $"自定义条件: {CustomCondition}",
            _ => "未知条件"
        };

        var trueBranch = !string.IsNullOrEmpty(TrueLabel) 
            ? $"#{TrueTargetIndex + 1} ({TrueLabel})" 
            : $"#{TrueTargetIndex + 1}";

        var falseBranch = FalseTargetIndex >= 0 
            ? (!string.IsNullOrEmpty(FalseLabel) 
                ? $"#{FalseTargetIndex + 1} ({FalseLabel})" 
                : $"#{FalseTargetIndex + 1}")
            : "继续执行";

        return $"条件跳转: {conditionDesc} → 真:{trueBranch}, 假:{falseBranch}";
    }
}

/// <summary>
/// 条件类型枚举
/// </summary>
public enum ConditionType
{
    /// <summary>
    /// 像素颜色检查
    /// </summary>
    PixelColor,
    
    /// <summary>
    /// 自定义条件表达式
    /// </summary>
    Custom
}