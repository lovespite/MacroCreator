namespace MacroCreator.Models;

public class KeyboardEvent : RecordedEvent
{
    public KeyboardAction Action { get; set; }
    public Keys Key { get; set; }

    public override string GetDescription()
    {
        return $"键盘 {Action}: {(Keys)Key}";
    }
}

