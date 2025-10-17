// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 中断事件 - 用于终止宏的执行
/// </summary>
public class BreakEvent : RecordedEvent
{
    public override string GetDescription()
    {
        return "中断执行";
    }
}
