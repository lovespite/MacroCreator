namespace MacroCreator.Models.Events;

public class KeyboardEvent : MacroEvent
{
    public KeyboardAction Action { get; set; }
    public Keys Key { get; set; }

    public override string GetDescription()
    {
        return $"键盘 {Action}: {Key}";
    }

    public override string TypeName => $"Kbd_{Action}";
}

