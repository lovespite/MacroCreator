using MacroCreator.Services;
using MacroCreator.Models;

namespace MacroCreator.Controller;

/// <summary>
/// MacroController (外观类)
/// 作为 UI 和后端服务之间的协调者
/// UI 层只与这个类交互
/// </summary>
public class MacroController
{
    private List<RecordedEvent> _events = [];
    private readonly RecordingService _recordingService;
    private readonly PlaybackService _playbackService;
    private string? _currentFilePath = null;

    public IReadOnlyList<RecordedEvent> EventSequence => _events.AsReadOnly();
    public AppState CurrentState { get; private set; } = AppState.Idle;
    public string? CurrentFilePath => _currentFilePath;

    public event Action<AppState>? StateChanged;
    public event Action<EventSequenceChangeArgs>? EventSequenceChanged;
    public event Action<string>? StatusMessageChanged;

    public MacroController()
    {
        _recordingService = new RecordingService();
        _recordingService.OnEventRecorded += (ev) =>
        {
            _events.Add(ev);
            EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Add, _events.Count - 1, ev));
        };

        // 使用策略模式初始化回放服务
        var playerStrategies = new Dictionary<Type, IEventPlayer>
        {
            { typeof(MouseEvent), new MouseEventPlayer() },
            { typeof(KeyboardEvent), new KeyboardEventPlayer() },
            { typeof(DelayEvent), new DelayEventPlayer() },
            { typeof(JumpEvent), new JumpEventPlayer() },
            { typeof(ConditionalJumpEvent), new ConditionalJumpEventPlayer() },
            { typeof(BreakEvent), new BreakEventPlayer() }
        };
        _playbackService = new PlaybackService(playerStrategies);
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
        try
        {
            _events = FileService.Load(filePath);
            _currentFilePath = filePath;
            EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.FullRefresh));
            StatusMessageChanged?.Invoke($"文件已加载: {Path.GetFileName(filePath)}");
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"打开文件失败: {ex.Message}", ex);
        }
    }

    public void SaveSequence(string? filePath = null)
    {
        var pathToSave = filePath ?? _currentFilePath;
        if (string.IsNullOrEmpty(pathToSave))
        {
            throw new InvalidOperationException("没有可保存的文件路径请使用“另存为”");
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

    public int IndexOfEvent(RecordedEvent ev)
    {
        return _events.IndexOf(ev);
    }

    public void AddEvent(RecordedEvent ev)
    {
        _events.Add(ev);
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Add, _events.Count - 1, ev));
    }

    public int InsertEventAt(int index, RecordedEvent ev)
    {
        _events.Insert(index, ev);
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Insert, index, ev));
        return index;
    }

    public int InsertEventBefore(RecordedEvent targetEvent, RecordedEvent newEvent)
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

    public RecordedEvent ReplaceEvent(int index, RecordedEvent newEvent)
    {
        var oldEvent = _events[index];
        _events[index] = newEvent;
        EventSequenceChanged?.Invoke(new EventSequenceChangeArgs(EventSequenceChangeType.Replace, index, newEvent));
        return oldEvent;
    }

    public void StartRecording()
    {
        if (CurrentState != AppState.Idle) return;
        _recordingService.Start();
        SetState(AppState.Recording);
        StatusMessageChanged?.Invoke("录制中... 按 F11 停止");
    }

    public async Task StartPlayback()
    {
        if (CurrentState != AppState.Idle) return;

        SetState(AppState.Playing);
        StatusMessageChanged?.Invoke("播放中... 按 F11 停止");
        try
        {
            await Playback();
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

    private async Task Playback()
    {
        var tcs = new TaskCompletionSource();
        Thread playbackThread = new(() =>
        {
            try
            {
                _playbackService.Play(_events, LoadAndPlayNewFile).GetAwaiter().GetResult();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

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
}
