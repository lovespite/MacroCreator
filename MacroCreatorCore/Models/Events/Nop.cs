namespace MacroCreator.Models.Events;

public class Nop : MacroEvent
{
    public Nop()
    {
    }

    public Nop(string name)
    {
        EventName = name;
    }

    public override string GetDescription() => "-";

    public override string TypeName => "(Label)";
}
