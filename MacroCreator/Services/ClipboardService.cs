// 命名空间定义了应用程序的入口点
using System.Diagnostics;
using System.Runtime.Versioning;

namespace MacroCreator.Services;

[SupportedOSPlatform("windows")]
internal class ClipboardService
{
    public bool Write(string text)
    {
        try
        {
            Clipboard.SetText(text);
            return true;
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine("Clipboard Write Error: " + ex.Message);
#endif
            return false;
        }
    }

    public string Read()
    {
        try
        {
            return Clipboard.GetText();
        }
        catch
        {
            return string.Empty;
        }
    }

    public bool WriteImage(Image image)
    {
        try
        {
            Clipboard.SetImage(image);
            return true;
        }
        catch
        {
            return false;
        }
    }
}