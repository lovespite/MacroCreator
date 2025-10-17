// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Native;

namespace MacroCreator.Services;

/// <summary>
/// 处理条件跳转事件的播放器
/// </summary>
public class ConditionalJumpEventPlayer : IEventPlayer
{
    public Task ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        if (ev is ConditionalJumpEvent conditionalJump)
        {
            bool conditionMet = EvaluateCondition(conditionalJump);
            
            int targetIndex;
            if (conditionMet)
            {
                targetIndex = conditionalJump.TrueTargetIndex;
            }
            else
            {
                // 如果 FalseTargetIndex 为 -1，表示继续执行下一个事件
                if (conditionalJump.FalseTargetIndex < 0)
                {
                    return Task.CompletedTask; // 不跳转，继续执行
                }
                targetIndex = conditionalJump.FalseTargetIndex;
            }
            
            // 设置跳转目标并抛出跳转异常
            context.SetJumpTarget(targetIndex);
            throw new SequenceJumpException();
        }
        
        return Task.CompletedTask;
    }

    private bool EvaluateCondition(ConditionalJumpEvent conditionalJump)
    {
        return conditionalJump.ConditionType switch
        {
            ConditionType.PixelColor => CheckPixelColor(conditionalJump.X, conditionalJump.Y, conditionalJump.ExpectedColorHex),
            ConditionType.Custom => EvaluateCustomCondition(conditionalJump.CustomCondition),
            _ => false
        };
    }

    private bool CheckPixelColor(int x, int y, string? expectedColorHex)
    {
        if (string.IsNullOrEmpty(expectedColorHex))
            return false;

        try
        {
            var actualColor = NativeMethods.GetPixelColor(x, y);
            var expectedColor = ColorTranslator.FromHtml(expectedColorHex);
            
            // 允许一定的颜色容差（RGB各通道差值小于等于5）
            const int tolerance = 5;
            return Math.Abs(actualColor.R - expectedColor.R) <= tolerance &&
                   Math.Abs(actualColor.G - expectedColor.G) <= tolerance &&
                   Math.Abs(actualColor.B - expectedColor.B) <= tolerance;
        }
        catch
        {
            return false;
        }
    }

    private bool EvaluateCustomCondition(string? condition)
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

    private bool EvaluateSimpleExpression(string expression)
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