using MacroCreator.Services;
using MacroCreator.Models;
using MacroCreator.Models.Events;
using System.Diagnostics;
using MacroCreator.Services.CH9329; // 导入 CH9329 命名空间

namespace MacroCreator.Controller;

/// <summary>
/// MacroController (外观类)
/// 作为 UI 和后端服务之间的协调者
/// UI 层只与这个类交互
/// </summary>
public class MacroController : IPrintService, IDisposable // 实现 IDisposable
{
    private List<MacroEvent> _events = [];
    private readonly RecordingService _recordingService;
    private readonly PlaybackService _playbackService;
    private string? _currentFilePath = null;

    // 添加 InputSimulator 实例和播放器字典字段
    private readonly InputSimulator? _inputSimulator;
    private readonly Dictionary<Type, IEventPlayer> _playerStrategies;

    public IReadOnlyList<MacroEvent> EventSequence => _events.AsReadOnly();
    public AppState CurrentState { get; private set; } = AppState.Idle;
    public string? CurrentFilePath => _currentFilePath;
    public InputSimulator? Simulator => _inputSimulator;
    public bool Redirected => _inputSimulator is not null;

    public event Action<AppState>? StateChanged;
    public event Action<EventSequenceChangeArgs>? EventSequenceChanged;
    public event Action<string>? StatusMessageChanged;
    public event Action<object?>? OnPrint;
    public event Action<object?>? OnPrintLine;

    /// <summary>
    /// 基础构造函数，用于 MacroCreator (WinForms)
    /// </summary>
    public MacroController() : this(redirectComPort: null)
    {
    }

    /// <summary>
    /// 用于 MacroScript 的构造函数 (旧)
    /// </summary>
    public MacroController(IReadOnlyList<MacroEvent> sequence) : this(sequence, null)
    {
    }

    /// <summary>
    /// 用于 MacroScript 的新构造函数，支持重定向
    /// </summary>
    public MacroController(IReadOnlyList<MacroEvent> sequence, string? redirectComPort) : this(redirectComPort)
    {
        _events = [.. sequence];
    }

    /// <summary>
    /// 核心构造函数，处理重定向逻辑
    /// </summary>
    /// <param name="redirectComPort">如果提供，则尝试重定向到此 COM 端口</param>
    public MacroController(string? redirectComPort = null)
    {
        _recordingService = new RecordingService();
        _recordingService.OnEventRecorded += _events.Add;

        if (!string.IsNullOrEmpty(redirectComPort))
        {
            _inputSimulator = new InputSimulator(redirectComPort);
            _inputSimulator.Open();
            _inputSimulator.Controller.WarmupCache(); // 预热缓存
            StatusMessageChanged?.Invoke($"键鼠操作已成功重定向到 {redirectComPort}");
        }

        // 使用 _inputSimulator 实例初始化播放器策略
        _playerStrategies = CreatePlayers(_inputSimulator);
        _playbackService = new PlaybackService(_playerStrategies, this);
    }

    /// <summary>
    /// 根据是否提供了 InputSimulator 来创建播放器策略
    /// </summary>
    private static Dictionary<Type, IEventPlayer> CreatePlayers(InputSimulator? simulator)
    {
        var players = new Dictionary<Type, IEventPlayer>
        {
            { typeof(Nop), new NopPlayer() },
            { typeof(ScriptEvent), new ScriptEventPlayer() },
            { typeof(DelayEvent), new DelayEventPlayer() },
            { typeof(JumpEvent), new JumpEventPlayer() },
            { typeof(ConditionalJumpEvent), new ConditionalJumpEventPlayer() },
            { typeof(BreakEvent), new BreakEventPlayer() },
        };

        if (simulator != null)
        {
            // 使用 CH9329 硬件播放器
            players[typeof(MouseEvent)] = new Ch9329MouseEventPlayer(simulator);
            players[typeof(KeyboardEvent)] = new Ch9329KeyboardEventPlayer(simulator);
        }
        else
        {
            // 使用本地输入模拟器播放器
            players[typeof(MouseEvent)] = new MouseEventPlayer();
            players[typeof(KeyboardEvent)] = new KeyboardEventPlayer();
        }
        return players;
    }

    public void Print(object? message)
    {
        OnPrint?.Invoke(message);
    }

    public void PrintLine(object? message)
    {
        OnPrintLine?.Invoke(message);
    }

    public void NewSequence()
    {
        _events.Clear();
        _currentFilePath = null;
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Clear));
        StatusMessageChanged?.Invoke("已创建新序列");
    }

    public void ClearSequence()
    {
        _events.Clear();
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Clear));
        StatusMessageChanged?.Invoke("序列已清空");
    }

    public void LoadSequence(string filePath)
    {
        _events.Clear();
        _events = FileService.Load(filePath);
        _currentFilePath = filePath;
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.FullRefresh));
        StatusMessageChanged?.Invoke($"文件已加载: {Path.GetFileName(filePath)}");
    }

    public void SaveSequence(string? filePath = null)
    {
        var pathToSave = filePath ?? _currentFilePath;
        if (string.IsNullOrEmpty(pathToSave))
        {
            throw new InvalidOperationException("没有可保存的文件路径");
        }

        try
        {
            FileService.Save(pathToSave, _events);
            _currentFilePath = pathToSave;
            StatusMessageChanged?.Invoke($"文件已保存到 {pathToSave}");
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"保存文件失败: {ex.Message}", ex);
        }
    }

    public int IndexOfEvent(MacroEvent ev)
    {
        return _events.IndexOf(ev);
    }

    public void AddEvent(MacroEvent ev)
    {
        _events.Add(ev);
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Add, _events.Count - 1, ev));
    }

    public int InsertEventAt(int index, MacroEvent ev)
    {
        _events.Insert(index, ev);
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Insert, index, ev));
        return index;
    }

    public int InsertEventBefore(MacroEvent targetEvent, MacroEvent newEvent)
    {
        int index = _events.IndexOf(targetEvent);
        if (index == -1)
        {
            throw new ArgumentException("目标事件不在序列中。", nameof(targetEvent));
        }
        _events.Insert(index, newEvent);
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Insert, index, newEvent));
        return index;
    }

    public void DeleteEventsAt(IEnumerable<int> indices)
    {
        var sortedIndices = new List<int>(indices);
        sortedIndices.Sort();
        for (int i = sortedIndices.Count - 1; i >= 0; i--)
        {
            _events.RemoveAt(sortedIndices[i]);
        }
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Delete, sortedIndices));
    }

    public MacroEvent ReplaceEvent(int index, MacroEvent newEvent)
    {
        var oldEvent = _events[index];
        _events[index] = newEvent;
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Replace, index, newEvent));
        return oldEvent;
    }

    public int MoveEventUp(int index)
    {
        if (index <= 0 || index >= _events.Count) return index;
        var targetEvent = _events[index];
        var aboveEvent = ReplaceEvent(index - 1, targetEvent);
        _events[index] = aboveEvent;

        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Update, index, aboveEvent));
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Update, index - 1, targetEvent));

        return index - 1;
    }

    public int MoveEventDown(int index)
    {
        if (index < 0 || index >= _events.Count - 1) return index;
        var targetEvent = _events[index];
        var belowEvent = ReplaceEvent(index + 1, targetEvent);
        _events[index] = belowEvent;

        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Update, index, belowEvent));
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Update, index + 1, targetEvent));

        return index + 1;
    }

    public void StartRecording()
    {
        if (CurrentState != AppState.Idle) return;
        _recordingService.Start();
        SetState(AppState.Recording);
        StatusMessageChanged?.Invoke("录制中... 按 F11 停止");
    }

    public async Task StartPlayback(MacroEvent? startFrom = null, MacroEvent? endTo = null)
    {
        if (CurrentState != AppState.Idle) return;

        if (_events.Count == 0) return;

        var startFromIndex = startFrom is not null ? IndexOfEvent(startFrom) : 0;
        ArgumentOutOfRangeException.ThrowIfLessThan(startFromIndex, 0, nameof(startFrom));

        var endToIndex = endTo is not null ? IndexOfEvent(endTo) : _events.Count - 1;
        ArgumentOutOfRangeException.ThrowIfLessThan(endToIndex, 0, nameof(endTo));
        if (endToIndex < startFromIndex)
        {
            throw new ArgumentException("结束事件必须在起始事件之后。", nameof(endTo));
        }
        var sequenceToPlay = _events[startFromIndex..(endToIndex + 1)];

        SetState(AppState.Playing);
        StatusMessageChanged?.Invoke("播放中... 按 F11 停止");
        try
        {
            await Playback(sequenceToPlay);
            StatusMessageChanged?.Invoke("播放完成");
        }
        catch (TaskCanceledException)
        {
            StatusMessageChanged?.Invoke("播放已停止");
        }
        finally
        {
            SetState(AppState.Idle);
        }
    }

    private async Task Playback(IReadOnlyList<MacroEvent> sequence)
    {
        var tcs = new TaskCompletionSource();
        Thread playbackThread = new(async () =>
        {
            try
            {
                await _playbackService.Play(sequence, LoadAndPlayNewFile);
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        playbackThread.TrySetApartmentState(ApartmentState.STA);
        playbackThread.Name = "PlaybackServiceThread";
        playbackThread.IsBackground = true;
        playbackThread.Start();

        await tcs.Task;
    }

    private async Task LoadAndPlayNewFile(string filePath)
    {
        StatusMessageChanged?.Invoke($"跳转到文件: {filePath} 并开始播放...");
        var newSequence = FileService.Load(filePath);
        await _playbackService.Play(newSequence, LoadAndPlayNewFile);
    }

    public void Stop()
    {
        if (CurrentState == AppState.Recording)
        {
            _recordingService.Stop();
            EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.FullRefresh, 0, null));
            StatusMessageChanged?.Invoke("录制结束");
        }
        else if (CurrentState == AppState.Playing)
        {
            _playbackService.Stop();
        }
        SetState(AppState.Idle);
    }

    private void SetState(AppState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        StateChanged?.Invoke(newState);
    }

    /// <summary>
    /// 释放 InputSimulator
    /// </summary>
    public void Dispose()
    {
        _inputSimulator?.Close();
        _inputSimulator?.Dispose();
        GC.SuppressFinalize(this);
    }
}
