using MacroCreator.Models;
using MacroCreator.Native;
using System.Diagnostics;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

/// <summary>
/// 负责录制键盘和鼠标事件
/// </summary>
public class RecordingService
{
    private static readonly Stopwatch _stopwatch = new Stopwatch();
    private long _lastEventTime = 0;

    public event Action<RecordedEvent> OnEventRecorded;

    public void Start()
    {
        InputHook.OnMouseEvent += OnMouseEvent;
        InputHook.OnKeyboardEvent += OnKeyboardEvent;
        InputHook.Install();
        _stopwatch.Restart();
        _lastEventTime = 0;
    }

    public void Stop()
    {
        InputHook.OnMouseEvent -= OnMouseEvent;
        InputHook.OnKeyboardEvent -= OnKeyboardEvent;
        InputHook.Uninstall();
        _stopwatch.Stop();
    }

    private void RecordEvent(RecordedEvent ev)
    {
        var currentTime = _stopwatch.ElapsedMilliseconds;
        ev.TimeSinceLastEvent = currentTime - _lastEventTime;
        _lastEventTime = currentTime;
        OnEventRecorded?.Invoke(ev);
    }

    private void OnKeyboardEvent(KeyboardAction action, Keys key)
    {
        RecordEvent(new KeyboardEvent { Action = action, Key = key });
    }

    private void OnMouseEvent(MouseAction action, int x, int y, int delta)
    {
        RecordEvent(new MouseEvent { Action = action, X = x, Y = y, WheelDelta = delta });
    }
}

