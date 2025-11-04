using System.Runtime.InteropServices;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Native;

/// <summary>
/// 存放所有 P/Invoke 的 Windows API 函数和结构体
/// </summary>
internal static partial class NativeMethods
{
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllocConsole();

    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FreeConsole();

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial IntPtr SetWindowsHookExA(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnhookWindowsHookEx(IntPtr hhk);

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    public static partial IntPtr GetModuleHandleA(string lpModuleName);

    [LibraryImport("gdi32.dll")]
    public static partial uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    [LibraryImport("user32.dll")]
    public static partial IntPtr GetDC(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    public static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(IntPtr hWnd, uint nCmdShow);

    public const uint WM_HOTKEY = 0x0312;

    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_NOREPEAT = 0x4000;

    public const uint SW_HIDE = 0;
    public const uint SW_SHOW = 5;

    [Flags]
    public enum ModifierKeys : uint
    {
        None = 0,
        Alt = MOD_ALT,
        Control = MOD_CONTROL,
        Shift = MOD_SHIFT,
        NoRepeat = MOD_NOREPEAT
    }

    public const int HOTKEY_ID_RECORD = 1;
    public const int HOTKEY_ID_PLAYBACK = 2;
    public const int HOTKEY_ID_STOP = 3;

    public static bool InstallHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys vk)
    {
        return RegisterHotKey(hWnd, id, (uint)fsModifiers, (uint)vk);
    }

    public static bool UninstallHotKey(IntPtr hWnd, int id)
    {
        return UnregisterHotKey(hWnd, id);
    }


    public static Color GetPixelColor(int x, int y)
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        uint pixel = GetPixel(hdc, x, y);
        ReleaseDC(IntPtr.Zero, hdc);
        int r = (int)(pixel & 0x000000FF);
        int g = (int)((pixel & 0x0000FF00) >> 8);
        int b = (int)((pixel & 0x00FF0000) >> 16);
        return Color.FromArgb(r, g, b);
    }

    public static void GetPixelColor(int x, int y, out byte r, out byte g, out byte b)
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        uint pixel = GetPixel(hdc, x, y);
        ReleaseDC(IntPtr.Zero, hdc);
        r = (byte)(pixel & 0x000000FF);
        g = (byte)((pixel & 0x0000FF00) >> 8);
        b = (byte)((pixel & 0x00FF0000) >> 16);
    }

    public static Color GetPixelColor(Point p)
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        uint pixel = GetPixel(hdc, p.X, p.Y);
        ReleaseDC(IntPtr.Zero, hdc);
        int r = (int)(pixel & 0x000000FF);
        int g = (int)((pixel & 0x0000FF00) >> 8);
        int b = (int)((pixel & 0x00FF0000) >> 16);
        return Color.FromArgb(r, g, b);
    }

    public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
}

