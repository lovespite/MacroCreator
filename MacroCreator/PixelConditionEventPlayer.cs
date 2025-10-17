// 命名空间定义了应用程序的入口点
using MacroCreator.Models;
using MacroCreator.Native;
using MacroCreator.Services;

namespace MacroCreator;

public class PixelConditionEventPlayer : IEventPlayer
{
    public async Task ExecuteAsync(RecordedEvent ev, PlaybackContext context)
    {
        var pce = (PixelConditionEvent)ev;
        Color expectedColor = ColorTranslator.FromHtml(pce.ExpectedColorHex);
        Color actualColor = GetPixelColor(pce.X, pce.Y);

        bool isMatch = actualColor.R == expectedColor.R && actualColor.G == expectedColor.G && actualColor.B == expectedColor.B;

        string fileToPlay = isMatch ? pce.FilePathIfMatch : pce.FilePathIfNotMatch;

        if (!string.IsNullOrEmpty(fileToPlay) && context.LoadAndPlayNewFileCallback != null)
        {
            await context.LoadAndPlayNewFileCallback(fileToPlay);
            // 抛出特定异常来终止当前序列的执行
            throw new SequenceJumpException();
        }
    }

    private Color GetPixelColor(int x, int y)
    {
        IntPtr hdc = NativeMethods.GetDC(IntPtr.Zero);
        uint pixel = NativeMethods.GetPixel(hdc, x, y);
        NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
        return Color.FromArgb((int)(pixel & 0x000000FF), (int)(pixel & 0x0000FF00) >> 8, (int)(pixel & 0x00FF0000) >> 16);
    }
}
