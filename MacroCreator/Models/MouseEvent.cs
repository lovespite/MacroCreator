namespace MacroCreator.Models;

public class MouseEvent : MacroEvent
{
    public MouseAction Action { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int WheelDelta { get; set; } // 用于滚轮事件

    public override string GetDescription()
    {
        if (Action == MouseAction.WheelScroll)
        {
            return $"鼠标滚轮: 滚动量 {WheelDelta} 在 ({X}, {Y})";
        }
        return $"鼠标 {Action} 在 ({X}, {Y})";
    }
}

