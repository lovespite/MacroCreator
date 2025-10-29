using System.Runtime.InteropServices;

namespace MacroScript;

internal static partial class Utils
{
    public static nint GetMainWindow()
    {
        // "CASCADIA_HOSTING_WINDOW_CLASS"
        return FindWindowA(null, Program.ConsoleTitle);
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ShowWindow(IntPtr hWnd, uint nCmdShow);
    public const uint SW_HIDE = 0;
    public const uint SW_SHOW = 5;

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint FindWindowA(string? lpClassName, string lpWindowName);
}