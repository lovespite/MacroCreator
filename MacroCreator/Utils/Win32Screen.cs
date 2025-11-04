using MacroCreator.Native;
using MacroCreator.Services;

namespace MacroCreator.Utils;

internal class Win32Screen : IDeviceScreenService
{
    public int GetPixelColor(int x, int y)
    {
        try
        {
            return NativeMethods.GetPixelColor(x, y).ToArgb();
        }
        catch
        {
            return Color.Empty.ToArgb();
        }
    }

    public int GetScreenCount()
    {
        return Screen.AllScreens.Length;
    }

    public void GetScreenSize(int index, out int width, out int height)
    {
        var screens = Screen.AllScreens;
        if (index < 0 || index >= screens.Length)
        {
            width = 0;
            height = 0;
            return;
        }
        var screen = screens[index];
        width = screen.Bounds.Width;
        height = screen.Bounds.Height;
    }
}
