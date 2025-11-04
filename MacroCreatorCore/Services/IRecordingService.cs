using MacroCreator.Models.Events;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

public interface IRecordingService
{
    event Action<MacroEvent>? OnEventRecorded;
    void Start();
    void Stop();
}