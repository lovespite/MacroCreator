// 命名空间定义了应用程序的入口点
using MacroCreator.Utils;
using System.Drawing;

namespace MacroCreator.Models.Events;

/// <summary>
/// 条件跳转事件 - 基于指定条件决定是否跳转
/// </summary>
public class ConditionalJumpEvent : FlowControlEvent
{
    /// <summary>
    /// 条件类型
    /// </summary>
    public ConditionType ConditionType { get; set; }

    public override string TypeName => "Branch";

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
    public int ExpectedColor { get; set; }

    /// <summary>
    /// 颜色容差值（当ConditionType为PixelColor时使用）
    /// </summary>
    public byte PixelTolerance { get; set; }

    /// <summary>
    /// 自定义条件表达式（当ConditionType为Custom时使用）
    /// </summary>
    public string? CustomCondition { get; set; }

    /// <summary>
    /// 条件为真时跳转的目标事件名称 - 优先使用名称跳转
    /// </summary>
    public string TrueTargetEventName { get; set; } = string.Empty;

    /// <summary>
    /// 条件为假时跳转的目标事件名称 - 优先使用名称跳转
    /// </summary>
    public string? FalseTargetEventName { get; set; }

    /// <summary>
    /// 条件为真时要执行的外部文件路径（可选）
    /// </summary>
    public string? TrueTargetFilePath { get; set; }

    /// <summary>
    /// 条件为假时要执行的外部文件路径（可选）
    /// </summary>
    public string? FalseTargetFilePath { get; set; }

    public override string GetDescription()
    {
        var conditionDesc = ConditionType switch
        {
            ConditionType.PixelColor => $"检查点({X},{Y})颜色是否为{GetColorDescription()}",
            ConditionType.CustomExpression => $"自定义条件: {CustomCondition}",
            _ => "未知条件"
        };

        var trueBranch = !string.IsNullOrEmpty(TrueTargetFilePath)
            ? $"执行文件: {Path.GetFileName(TrueTargetFilePath)}"
            : $"跳转事件: {TrueTargetEventName}";

        var falseBranch = !string.IsNullOrEmpty(FalseTargetFilePath)
            ? $"执行文件: {Path.GetFileName(FalseTargetFilePath)}"
            : !string.IsNullOrEmpty(FalseTargetEventName)
                ? $"跳转事件: {FalseTargetEventName}"
                : "继续执行";

        return $"条件跳转: {conditionDesc} → 真:{trueBranch}, 假:{falseBranch}";
    }

    private string GetColorDescription()
    {
        Color color = Color.FromArgb(ExpectedColor);
        var expr = color.ExpressAsRgbColor();
        if (PixelTolerance > 0)
        {
            expr += $"[±{PixelTolerance}]";
        }
        return expr;
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
    CustomExpression
}