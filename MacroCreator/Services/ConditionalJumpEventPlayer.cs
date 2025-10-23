// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Native;

namespace MacroCreator.Services;

/// <summary>
/// 处理条件跳转事件的播放器
/// </summary>
public class ConditionalJumpEventPlayer : IEventPlayer
{

    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        var position = context.CurrentEventIndex;
        var ev = context.CurrentEvent;

        if (ev is not ConditionalJumpEvent conditionalJump)
        {
            return Task.FromResult(PlaybackResult.Continue());
        }

        bool conditionMet = EvaluateCondition(context, conditionalJump);

        // 检查是否需要执行外部文件
        string? filePath = conditionMet ? conditionalJump.TrueTargetFilePath : conditionalJump.FalseTargetFilePath;

        if (filePath is not null)
        {
            if (!File.Exists(filePath))
            {
                throw new EventFlowControlException($"指定的文件不存在: {filePath}", ev, position);
            }

            // 返回跳转到外部文件的结果
            return Task.FromResult(PlaybackResult.JumpToFile(filePath));
        }

        // 如果没有外部文件，则执行序列内跳转

        string? targetEventName = conditionMet
            ? conditionalJump.TrueTargetEventName
            : conditionalJump.FalseTargetEventName;

        var targetIndex = context.FindEventIndexByName(targetEventName);

        // 如果指定了目标事件名称，优先使用名称查找
        if (conditionMet)
        {
            if (targetIndex < 0)
            {
                throw new EventFlowControlException($"无效的跳转目标: '{targetEventName}'", ev, position);
            }
            // 返回跳转结果
            return Task.FromResult(PlaybackResult.Jump(targetIndex));
        }
        else
        {
            if (targetEventName is not null && targetIndex < 0)
            {
                throw new EventFlowControlException($"无效的跳转目标: '{targetEventName}'", ev, position);
            }
            return targetIndex >= 0
                ? Task.FromResult(PlaybackResult.Jump(targetIndex))
                : Task.FromResult(PlaybackResult.Continue());
        }
    }

    private static bool EvaluateCondition(PlaybackContext context, ConditionalJumpEvent conditionalJump)
    {
        return conditionalJump.ConditionType switch
        {
            ConditionType.PixelColor => CheckPixelColor(conditionalJump.X, conditionalJump.Y, conditionalJump.ExpectedColor, conditionalJump.PixelTolerance),
            ConditionType.CustomExpression => Evaluate(context, conditionalJump),
            _ => false
        };
    }

    private static bool CheckPixelColor(int x, int y, int expectedColorArgb, byte tolerance = 0)
    {
        try
        {
            var actualColor = NativeMethods.GetPixelColor(x, y);
            var expectedColor = Color.FromArgb(expectedColorArgb);

            // 允许一定的颜色容差（RGB各通道差值小于等于5） 
            return Math.Abs(actualColor.R - expectedColor.R) <= tolerance &&
                   Math.Abs(actualColor.G - expectedColor.G) <= tolerance &&
                   Math.Abs(actualColor.B - expectedColor.B) <= tolerance;
        }
        catch
        {
            return false;
        }
    }

    private static bool Evaluate(PlaybackContext context, ConditionalJumpEvent @event)
    {
        if (string.IsNullOrWhiteSpace(@event.CustomCondition)) return false;
        var evaluator = context.GetEvaluator(@event);

        if (evaluator == null) return false;
        return evaluator();
    }
}