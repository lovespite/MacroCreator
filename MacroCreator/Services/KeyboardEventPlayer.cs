// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Native;

namespace MacroCreator.Services;

public class KeyboardEventPlayer : IEventPlayer
{
    public Task ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        var ke = (KeyboardEvent)ev;
        byte vk = (byte)ke.Key;
        uint scanCode = NativeMethods.MapVirtualKey(vk, 0);
        uint flags = ke.Action == KeyboardAction.KeyDown ? 0 : NativeMethods.KEYEVENTF_KEYUP;
        NativeMethods.keybd_event(vk, (byte)scanCode, flags, nuint.Zero);
        return Task.CompletedTask;
    }
} 

