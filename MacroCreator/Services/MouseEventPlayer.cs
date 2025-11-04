// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Native;
using System.Runtime.InteropServices;

namespace MacroCreator.Services;

[Obsolete("使用SimulatorMouseEventPlayer替代")]
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
                flags = Win32.MOUSEEVENTF_MOVE;
                dx = me.X;
                dy = me.Y;
                break;
            case MouseAction.MoveTo:
                Win32.SetCursorPos(me.X, me.Y);
                break;
            case MouseAction.LeftDown:
                flags = Win32.MOUSEEVENTF_LEFTDOWN;
                break;
            case MouseAction.LeftUp:
                flags = Win32.MOUSEEVENTF_LEFTUP;
                break;
            case MouseAction.RightDown:
                flags = Win32.MOUSEEVENTF_RIGHTDOWN;
                break;
            case MouseAction.RightUp:
                flags = Win32.MOUSEEVENTF_RIGHTUP;
                break;
            case MouseAction.MiddleDown:
                flags = Win32.MOUSEEVENTF_MIDDLEDOWN;
                break;
            case MouseAction.MiddleUp:
                flags = Win32.MOUSEEVENTF_MIDDLEUP;
                break;
            case MouseAction.Wheel:
                flags = Win32.MOUSEEVENTF_WHEEL;
                mouseData = (uint)me.WheelDelta;
                break;
        }

        if (flags == 0) return Task.FromResult(PlaybackResult.Continue());

        var input = new Win32.INPUT
        {
            type = Win32.INPUT_MOUSE,
            u = new Win32.InputUnion
            {
                mi = new Win32.MOUSEINPUT
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

        Win32.SendInput(1, [input], Marshal.SizeOf<Win32.INPUT>());

        return Task.FromResult(PlaybackResult.Continue());
    }
}