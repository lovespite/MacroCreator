using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Native;
using System.Diagnostics;
using Keys = MacroCreator.Models.Keys;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

/// <summary>
/// 负责录制键盘和鼠标事件，使用高精度计时器
/// </summary>
public class Win32RecordingService : IRecordingService
{
    private readonly HighPrecisionTimer _timer;
    private double _lastEventTime = 0;

    public event Action<MacroEvent>? OnEventRecorded;

    public Win32RecordingService()
    {
        _timer = new HighPrecisionTimer();
    }

    public void Start()
    {
        InputHook.OnMouseEvent += OnMouseEvent;
        InputHook.OnKeyboardEvent += OnKeyboardEvent;
        InputHook.Install();
        _lastEventTime = _timer.GetPreciseMilliseconds();
    }

    public void Stop()
    {
        InputHook.OnMouseEvent -= OnMouseEvent;
        InputHook.OnKeyboardEvent -= OnKeyboardEvent;
        InputHook.Uninstall();
    }

    private void RecordEvent(MacroEvent ev)
    {
        var currentTime = _timer.GetPreciseMilliseconds();
        ev.TimeSinceLastEvent = currentTime - _lastEventTime;
        ev.Timestamp = currentTime;

        _lastEventTime = currentTime;
        OnEventRecorded?.Invoke(ev);
    }

    public void Dispose()
    {
        Stop();
        _timer?.Dispose();
    }

    private void OnKeyboardEvent(KeyboardAction action, Keys key)
    {
        RecordEvent(new KeyboardEvent { Action = action, Key = key });

#if DEBUG
        Debug.WriteLine($"Mouse Event: Action={action}, Key={key}");
#endif
    }

    private void OnMouseEvent(MouseAction action, int x, int y, int delta)
    {
        RecordEvent(new MouseEvent { Action = action, X = x, Y = y, WheelDelta = delta });

#if DEBUG
        Debug.WriteLine($"Mouse Event: Action={action}, X={x}, Y={y}, Delta={delta}");
#endif
    }
}
