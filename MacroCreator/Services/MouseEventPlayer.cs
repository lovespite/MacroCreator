// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Native;
using System.Runtime.InteropServices;

namespace MacroCreator.Services;

public class MouseEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        var me = (MouseEvent)context.CurrentEvent;

        uint flags = 0, mouseData = 0;
        int dx = 0, dy = 0;

        switch (me.Action)
        {
            case MouseAction.Move:
                flags = NativeMethods.MOUSEEVENTF_MOVE;
                dx = me.X;
                dy = me.Y;
                break;
            case MouseAction.MoveTo:
                NativeMethods.SetCursorPos(me.X, me.Y);
                break;
            case MouseAction.LeftDown:
                flags = NativeMethods.MOUSEEVENTF_LEFTDOWN;
                break;
            case MouseAction.LeftUp:
                flags = NativeMethods.MOUSEEVENTF_LEFTUP;
                break;
            case MouseAction.RightDown:
                flags = NativeMethods.MOUSEEVENTF_RIGHTDOWN;
                break;
            case MouseAction.RightUp:
                flags = NativeMethods.MOUSEEVENTF_RIGHTUP;
                break;
            case MouseAction.MiddleDown:
                flags = NativeMethods.MOUSEEVENTF_MIDDLEDOWN;
                break;
            case MouseAction.MiddleUp:
                flags = NativeMethods.MOUSEEVENTF_MIDDLEUP;
                break;
            case MouseAction.Wheel:
                flags = NativeMethods.MOUSEEVENTF_WHEEL;
                mouseData = (uint)me.WheelDelta;
                break;
        }

        if (flags == 0) return Task.FromResult(PlaybackResult.Continue());

        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_MOUSE,
            u = new NativeMethods.InputUnion
            {
                mi = new NativeMethods.MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = mouseData,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = nint.Zero
                }
            }
        };

        NativeMethods.SendInput(1, [input], Marshal.SizeOf<NativeMethods.INPUT>());

        return Task.FromResult(PlaybackResult.Continue());
    }
}