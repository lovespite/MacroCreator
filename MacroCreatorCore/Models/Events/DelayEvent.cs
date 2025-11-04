using MacroCreator.Models.Events;

namespace MacroCreator.Models;

public class DelayEvent : MacroEvent
{
    public int DelayMilliseconds { get; set; }
    public override string GetDescription()
    {
        return $"延迟 {DelayMilliseconds} 毫秒";
    }

    public override string TypeName => "Delay";
}

