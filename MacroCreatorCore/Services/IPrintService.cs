// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

public interface IPrintService
{
    void Print(object? message);
    void PrintLine(object? message);
}