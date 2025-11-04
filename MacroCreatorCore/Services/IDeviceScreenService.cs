// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

public interface IDeviceScreenService
{
    int GetPixelColor(int x, int y);
    void GetScreenSize(int index, out int width, out int height);
    int GetScreenCount();
}