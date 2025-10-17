// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Native;
using MacroCreator.Services;
using System.Runtime.InteropServices;

namespace MacroCreator;

public class MouseEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        var me = (MouseEvent)ev;
        
        // Set cursor position first
        NativeMethods.SetCursorPos(me.X, me.Y);

        uint flags = 0;
        uint mouseData = 0;
        
        switch (me.Action)
        {
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
            case MouseAction.WheelScroll:
                flags = NativeMethods.MOUSEEVENTF_WHEEL;
                mouseData = (uint)me.WheelDelta;
                break;
        }

        if (flags != 0)
        {
            var input = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_MOUSE,
                u = new NativeMethods.InputUnion
                {
                    mi = new NativeMethods.MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = mouseData,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            var inputs = new[] { input };
            NativeMethods.SendInput(1, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }
        
        return Task.FromResult(PlaybackResult.Continue());
    }
}