// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Native;
using System.Runtime.InteropServices;

namespace MacroCreator.Services;

[Obsolete("使用 SimulatorKeyboardEventPlayer 替代此类，以获得更好的兼容性和可维护性。")]
public class KeyboardEventPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext context)
    {
        var ke = (KeyboardEvent)context.CurrentEvent;
        ushort vk = (ushort)ke.Key;
        ushort scanCode = (ushort)Win32.MapVirtualKeyA(vk, 0);
        uint flags = ke.Action == KeyboardAction.KeyDown ? 0 : Win32.KEYEVENTF_KEYUP;

        var input = new Win32.INPUT
        {
            type = Win32.INPUT_KEYBOARD,
            u = new Win32.InputUnion
            {
                ki = new Win32  .KEYBDINPUT
                {
                    wVk = vk,
                    wScan = scanCode,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        Win32.SendInput(1, [input], Marshal.SizeOf<Win32.INPUT>());
        return Task.FromResult(PlaybackResult.Continue());
    }
}

