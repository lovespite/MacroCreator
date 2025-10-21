namespace MacroCreator.Models;

public class EventPlayerException(string message, RecordedEvent @event, int index) : Exception(message)
{
    public RecordedEvent Event { get; } = @event;
    public int EventIndex { get; } = index;
}

public class EventFlowControlException(string message, RecordedEvent @event, int index) : EventPlayerException(message, @event, index)
{
}