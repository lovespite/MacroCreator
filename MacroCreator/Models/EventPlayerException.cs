namespace MacroCreator.Models;

public class EventPlayerException(string message, MacroEvent @event, int index) : Exception(message)
{
    public MacroEvent Event { get; } = @event;
    public int EventIndex { get; } = index;
}

public class EventFlowControlException(string message, MacroEvent @event, int index) : EventPlayerException(message, @event, index)
{
}