using MacroCreator.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Native;

/// <summary>
/// 封装全局钩子逻辑
/// </summary>
public static class InputHook
{
    public delegate void MouseEventDelegate(MouseAction action, int x, int y, int delta);
    public static event MouseEventDelegate? OnMouseEvent;

    public delegate void KeyboardEventDelegate(KeyboardAction action, Models.Keys key);
    public static event KeyboardEventDelegate? OnKeyboardEvent;

    private static NativeMethods.HookProc? _mouseHookProc;
    private static IntPtr _mouseHookID = IntPtr.Zero;

    private static NativeMethods.HookProc? _keyboardHookProc;
    private static IntPtr _keyboardHookID = IntPtr.Zero;

    public static void Install()
    {
        _mouseHookProc = MouseHookCallback;
        _mouseHookID = SetHook(Win32.WH_MOUSE_LL, _mouseHookProc);

        _keyboardHookProc = KeyboardHookCallback;
        _keyboardHookID = SetHook(Win32.WH_KEYBOARD_LL, _keyboardHookProc);
    }

    public static void Uninstall()
    {
        NativeMethods.UnhookWindowsHookEx(_mouseHookID);
        NativeMethods.UnhookWindowsHookEx(_keyboardHookID);
    }

    private static IntPtr SetHook(int hookID, NativeMethods.HookProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using ProcessModule? curModule = curProcess.MainModule ?? throw new InvalidOperationException("无法获取当前模块信息");

        return NativeMethods.SetWindowsHookExA(hookID, proc, NativeMethods.GetModuleHandleA(curModule.ModuleName), 0);
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var hookStruct = Marshal.PtrToStructure<Win32.MSLLHOOKSTRUCT>(lParam);
            int x = 0, y = 0;
            int delta = 0;

            MouseAction action;
            switch ((uint)wParam)
            {
                case Win32.WM_LBUTTONDOWN: action = MouseAction.LeftDown; break;
                case Win32.WM_LBUTTONUP: action = MouseAction.LeftUp; break;
                case Win32.WM_RBUTTONDOWN: action = MouseAction.RightDown; break;
                case Win32.WM_RBUTTONUP: action = MouseAction.RightUp; break;
                case Win32.WM_MBUTTONDOWN: action = MouseAction.MiddleDown; break;
                case Win32.WM_MBUTTONUP: action = MouseAction.MiddleUp; break;
                case Win32.WM_MOUSEWHEEL:
                    action = MouseAction.Wheel;
                    delta = (short)((hookStruct.mouseData >> 16) & 0xffff);
                    break;
                case Win32.WM_MOUSEMOVE:
                default:
                    action = MouseAction.MoveTo;
                    x = hookStruct.pt.x;
                    y = hookStruct.pt.y;
                    break;
            }
            OnMouseEvent?.Invoke(action, x, y, delta);
        }
        return NativeMethods.CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }

    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Models.Keys key = (Models.Keys)vkCode;

            if ((uint)wParam == Win32.WM_KEYDOWN || (uint)wParam == Win32.WM_SYSKEYDOWN)
            {
                OnKeyboardEvent?.Invoke(KeyboardAction.KeyDown, key);
            }
            else if ((uint)wParam == Win32.WM_KEYUP || (uint)wParam == Win32.WM_SYSKEYUP)
            {
                OnKeyboardEvent?.Invoke(KeyboardAction.KeyUp, key);
            }
        }
        return NativeMethods.CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }
}
