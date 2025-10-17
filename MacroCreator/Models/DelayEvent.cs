namespace MacroCreator.Models;

public class DelayEvent : RecordedEvent
{
    public int DelayMilliseconds { get; set; }
    public override string GetDescription()
    {
        return $"延迟 {DelayMilliseconds} 毫秒";
    }
}

