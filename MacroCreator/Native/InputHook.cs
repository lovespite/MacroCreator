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

    public delegate void KeyboardEventDelegate(KeyboardAction action, Keys key);
    public static event KeyboardEventDelegate? OnKeyboardEvent;

    private static NativeMethods.HookProc? _mouseHookProc;
    private static IntPtr _mouseHookID = IntPtr.Zero;

    private static NativeMethods.HookProc? _keyboardHookProc;
    private static IntPtr _keyboardHookID = IntPtr.Zero;

    public static void Install()
    {
        _mouseHookProc = MouseHookCallback;
        _mouseHookID = SetHook(NativeMethods.WH_MOUSE_LL, _mouseHookProc);

        _keyboardHookProc = KeyboardHookCallback;
        _keyboardHookID = SetHook(NativeMethods.WH_KEYBOARD_LL, _keyboardHookProc);
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
            var hookStruct = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
            int x = hookStruct.pt.x;
            int y = hookStruct.pt.y;
            int delta = 0;

            MouseAction action;
            switch ((uint)wParam)
            {
                case NativeMethods.WM_LBUTTONDOWN: action = MouseAction.LeftDown; break;
                case NativeMethods.WM_LBUTTONUP: action = MouseAction.LeftUp; break;
                case NativeMethods.WM_RBUTTONDOWN: action = MouseAction.RightDown; break;
                case NativeMethods.WM_RBUTTONUP: action = MouseAction.RightUp; break;
                case NativeMethods.WM_MBUTTONDOWN: action = MouseAction.MiddleDown; break;
                case NativeMethods.WM_MBUTTONUP: action = MouseAction.MiddleUp; break;
                case NativeMethods.WM_MOUSEWHEEL:
                    action = MouseAction.Wheel;
                    delta = (short)((hookStruct.mouseData >> 16) & 0xffff);
                    break;
                case NativeMethods.WM_MOUSEMOVE:
                default:
                    action = MouseAction.Move;
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
            Keys key = (Keys)vkCode;

            if ((uint)wParam == NativeMethods.WM_KEYDOWN || (uint)wParam == NativeMethods.WM_SYSKEYDOWN)
            {
                OnKeyboardEvent?.Invoke(KeyboardAction.KeyDown, key);
            }
            else if ((uint)wParam == NativeMethods.WM_KEYUP || (uint)wParam == NativeMethods.WM_SYSKEYUP)
            {
                OnKeyboardEvent?.Invoke(KeyboardAction.KeyUp, key);
            }
        }
        return NativeMethods.CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }
}
