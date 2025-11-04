using MacroCreator.Models;
using MacroCreator.Native;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace MacroCreator.Services;

[SupportedOSPlatform("windows")]
public class Win32LocalMachineSimulator : SimulatorBase
{
    public static readonly int INPUT_SIZE = Marshal.SizeOf<Win32.INPUT>();

    private readonly HashSet<Keys> _pressedKeys = [];
    public override string Name => "Win32本机模拟器";
    public override Task MouseMove(int dx, int dy)
    {
        uint flags = Win32.MOUSEEVENTF_MOVE;

        var input = new Win32.INPUT
        {
            type = Win32.INPUT_MOUSE,
            u = new Win32.InputUnion
            {
                mi = new Win32.MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = nint.Zero
                }
            }
        };

        Win32.SendInput(1, [input], INPUT_SIZE);
        return Task.CompletedTask;
    }

    public override Task MouseMoveTo(int x, int y)
    {
        Win32.SetCursorPos(x, y);
        return Task.CompletedTask;
    }

    public override Task MouseDown(MouseButton button)
    {
        uint flags = 0;
        if (button.HasFlag(MouseButton.Left))
            flags |= Win32.MOUSEEVENTF_LEFTDOWN;
        if (button.HasFlag(MouseButton.Right))
            flags |= Win32.MOUSEEVENTF_RIGHTDOWN;
        if (button.HasFlag(MouseButton.Middle))
            flags |= Win32.MOUSEEVENTF_MIDDLEDOWN;

        var input = new Win32.INPUT
        {
            type = Win32.INPUT_MOUSE,
            u = new Win32.InputUnion
            {
                mi = new Win32.MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = nint.Zero
                }
            }
        };

        Win32.SendInput(1, [input], INPUT_SIZE);
        return Task.CompletedTask;
    }

    public override Task MouseUp(MouseButton button)
    {
        uint flags = 0;
        if (button.HasFlag(MouseButton.Left))
            flags |= Win32.MOUSEEVENTF_LEFTUP;
        if (button.HasFlag(MouseButton.Right))
            flags |= Win32.MOUSEEVENTF_RIGHTUP;
        if (button.HasFlag(MouseButton.Middle))
            flags |= Win32.MOUSEEVENTF_MIDDLEUP;

        var input = new Win32.INPUT
        {
            type = Win32.INPUT_MOUSE,
            u = new Win32.InputUnion
            {
                mi = new Win32.MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = flags,
                    time = 0,
                    dwExtraInfo = nint.Zero
                }
            }
        };

        Win32.SendInput(1, [input], INPUT_SIZE);
        return Task.CompletedTask;
    }

    public override Task MouseWheel(int amount)
    {
        var input = new Win32.INPUT
        {
            type = Win32.INPUT_MOUSE,
            u = new Win32.InputUnion
            {
                mi = new Win32.MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = (uint)amount,
                    dwFlags = Win32.MOUSEEVENTF_WHEEL,
                    time = 0,
                    dwExtraInfo = nint.Zero
                }
            }
        };

        Win32.SendInput(1, [input], INPUT_SIZE);
        return Task.CompletedTask;
    }

    public override Task KeyDown(params Keys[] keys)
    {
        foreach (var k in keys)
            _pressedKeys.Add(k);

        Keys[] pressedKeys = [.. _pressedKeys];
        var inputs = new Win32.INPUT[pressedKeys.Length];

        for (var i = 0; i < inputs.Length; i++)
        {
            var key = pressedKeys[i];
            ushort vk = (ushort)key;
            ushort scanCode = (ushort)Win32.MapVirtualKeyA(vk, 0);

            inputs[i] = new Win32.INPUT
            {
                type = Win32.INPUT_KEYBOARD,
                u = new Win32.InputUnion
                {
                    ki = new Win32.KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = scanCode,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = nint.Zero
                    }
                }
            };
        }

        Win32.SendInput((uint)inputs.Length, inputs, INPUT_SIZE);

        return Task.CompletedTask;
    }

    public override Task KeyUp(params Keys[] keys)
    {
        var releasedKeys = new HashSet<Keys>();
        foreach (var k in keys)
        {
            releasedKeys.Add(k);
            _pressedKeys.Remove(k);
        }

        Keys[] releasedKeysArr = [.. releasedKeys];
        var inputs = new Win32.INPUT[releasedKeysArr.Length];

        for (var i = 0; i < inputs.Length; i++)
        {
            var key = releasedKeysArr[i];
            ushort vk = (ushort)key;
            ushort scanCode = (ushort)Win32.MapVirtualKeyA(vk, 0);

            inputs[i] = new Win32.INPUT
            {
                type = Win32.INPUT_KEYBOARD,
                u = new Win32.InputUnion
                {
                    ki = new Win32.KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = scanCode,
                        dwFlags = Win32.KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = nint.Zero
                    }
                }
            };
        }

        Win32.SendInput((uint)inputs.Length, inputs, INPUT_SIZE);

        return Task.CompletedTask;
    }

    public override Task ReleaseAllKeys()
    {
        if (_pressedKeys.Count == 0)
            return Task.CompletedTask;

        Keys[] keys = [.. _pressedKeys];
        _pressedKeys.Clear();

        var inputs = new Win32.INPUT[keys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            ushort vk = (ushort)keys[i];
            ushort scanCode = (ushort)Win32.MapVirtualKeyA(vk, 0);
            inputs[i] = new Win32.INPUT
            {
                type = Win32.INPUT_KEYBOARD,
                u = new Win32.InputUnion
                {
                    ki = new Win32.KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = scanCode,
                        dwFlags = Win32.KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = nint.Zero
                    }
                }
            };
        }

        Win32.SendInput((uint)inputs.Length, inputs, INPUT_SIZE);
        return Task.CompletedTask;
    }

    public override Task ReleaseAllMouse() => throw new NotSupportedException();

    public override void SetScreenResolution(int width, int height) { }
}