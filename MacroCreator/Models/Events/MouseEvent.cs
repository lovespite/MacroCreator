namespace MacroCreator.Models.Events;

public class MouseEvent : MacroEvent
{
    public MouseAction Action { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int WheelDelta { get; set; } // 用于滚轮事件

    public override string GetDescription()
    {
        if (Action == MouseAction.Wheel)
        {
            return $"鼠标滚轮滚动 {WheelDelta}";
        }
        else if (Action == MouseAction.Move)
        {
            return $"鼠标 {Action} ({X}, {Y})";
        }
        else if (Action == MouseAction.MoveTo)
        {
            return $"鼠标 {Action} 到 ({X}, {Y})";
        }
        else
        {
            return $"鼠标 {Action}";
        }
    }

    public override string TypeName => $"Mouse_{Action}";
}

