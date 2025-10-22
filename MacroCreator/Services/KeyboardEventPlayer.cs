// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Native;
using System.Runtime.InteropServices;

namespace MacroCreator.Services;

public class KeyboardEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        var ke = (KeyboardEvent)context.CurrentEvent;
        ushort vk = (ushort)ke.Key;
        ushort scanCode = (ushort)NativeMethods.MapVirtualKeyA(vk, 0);
        uint flags = ke.Action == KeyboardAction.KeyDown ? 0 : NativeMethods.KEYEVENTF_KEYUP;

        var input = new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            u = new NativeMethods.InputUnion
            {
                ki = new NativeMethods.KEYBDINPUT
                {
                    wVk = vk,
                    wScan = scanCode,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        NativeMethods.SendInput(1, [input], Marshal.SizeOf<NativeMethods.INPUT>());
        return Task.FromResult(PlaybackResult.Continue());
    }
}

