namespace MacroCreator.Models;

public enum MouseAction
{
    Move,
    LeftDown,
    LeftUp,
    RightDown,
    RightUp,
    MiddleDown,
    MiddleUp,
    Wheel,
    Unknown,
}

public static class MouseActionExtensions
{
    public static string ToFriendlyString(this MouseAction action)
    {
        return action switch
        {
            MouseAction.Move => "鼠标移动",
            MouseAction.LeftDown => "左键按下",
            MouseAction.LeftUp => "左键抬起",
            MouseAction.RightDown => "右键按下",
            MouseAction.RightUp => "右键抬起",
            MouseAction.MiddleDown => "中键按下",
            MouseAction.MiddleUp => "中键抬起",
            MouseAction.Wheel => "滚轮滚动",
            _ => action.ToString(),
        };
    }

    public static MouseAction GetPairedAction(this MouseAction action)
    {
        return action switch
        {
            MouseAction.LeftDown => MouseAction.LeftUp,
            MouseAction.RightDown => MouseAction.RightUp,
            MouseAction.MiddleDown => MouseAction.MiddleUp,
            _ => MouseAction.Unknown,
        };
    }

    public static bool IsDownAction(this MouseAction action)
    {
        return action == MouseAction.LeftDown ||
               action == MouseAction.RightDown ||
               action == MouseAction.MiddleDown;
    }
}