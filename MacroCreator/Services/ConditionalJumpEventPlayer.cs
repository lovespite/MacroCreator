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

        bool conditionMet = EvaluateCondition(conditionalJump);

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

    private static bool EvaluateCondition(ConditionalJumpEvent conditionalJump)
    {
        return conditionalJump.ConditionType switch
        {
            ConditionType.PixelColor => CheckPixelColor(conditionalJump.X, conditionalJump.Y, conditionalJump.ExpectedColor, conditionalJump.PixelTolerance),
            ConditionType.CustomExpression => EvaluateCustomCondition(conditionalJump.CustomCondition),
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

    private static bool EvaluateCustomCondition(string? condition)
    {
        // 简单的自定义条件评估
        // 这里可以扩展为更复杂的表达式评估器
        if (string.IsNullOrEmpty(condition))
            return false;

        try
        {
            // 支持简单的条件表达式，如时间、随机数等
            condition = condition.ToLower().Trim();

            if (condition == "true")
                return true;
            if (condition == "false")
                return false;

            // 支持时间条件，如 "hour >= 9 && hour <= 17"
            if (condition.Contains("hour"))
            {
                var hour = DateTime.Now.Hour;
                condition = condition.Replace("hour", hour.ToString());
                return EvaluateSimpleExpression(condition);
            }

            // 支持随机条件，如 "random > 0.5"
            if (condition.Contains("random"))
            {
                var random = new Random().NextDouble();
                condition = condition.Replace("random", random.ToString("F2"));
                return EvaluateSimpleExpression(condition);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool EvaluateSimpleExpression(string expression)
    {
        // 非常简单的表达式评估器，仅支持基本的比较运算
        // 在实际应用中，可以使用更强大的表达式评估库
        try
        {
            // 这里简化处理，实际应用中建议使用专门的表达式评估库
            // 如 System.Data.DataTable.Compute 或第三方库
            var table = new System.Data.DataTable();
            var result = table.Compute(expression, null);
            return Convert.ToBoolean(result);
        }
        catch
        {
            return false;
        }
    }
}