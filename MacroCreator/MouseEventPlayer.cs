// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Native;
using MacroCreator.Services;

namespace MacroCreator;

public class MouseEventPlayer : IEventPlayer
{
    public Task ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        var me = (MouseEvent)ev;
        NativeMethods.SetCursorPos(me.X, me.Y);
        uint flags = 0;
        uint mouseData = 0;
        switch (me.Action)
        {
            case MouseAction.LeftDown: flags = NativeMethods.MOUSEEVENTF_LEFTDOWN; break;
            case MouseAction.LeftUp: flags = NativeMethods.MOUSEEVENTF_LEFTUP; break;
            case MouseAction.RightDown: flags = NativeMethods.MOUSEEVENTF_RIGHTDOWN; break;
            case MouseAction.RightUp: flags = NativeMethods.MOUSEEVENTF_RIGHTUP; break;
            case MouseAction.MiddleDown: flags = NativeMethods.MOUSEEVENTF_MIDDLEDOWN; break;
            case MouseAction.MiddleUp: flags = NativeMethods.MOUSEEVENTF_MIDDLEUP; break;
            case MouseAction.WheelScroll:
                flags = NativeMethods.MOUSEEVENTF_WHEEL;
                mouseData = (uint)me.WheelDelta;
                break;
        }
        if (flags != 0)
        {
            NativeMethods.mouse_event(flags, 0, 0, mouseData, UIntPtr.Zero);
        }
        return Task.CompletedTask;
    }
}