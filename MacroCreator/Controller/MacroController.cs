using MacroCreator.Services;
using MacroCreator.Models;

namespace MacroCreator.Controller;

/// <summary>
/// MacroController (外观类)
/// 作为 UI 和后端服务之间的协调者。
/// UI 层只与这个类交互。
/// </summary>
public class MacroController
{
    private List<RecordedEvent> _eventSequence = [];
    private readonly RecordingService _recordingService;
    private readonly PlaybackService _playbackService;
    private string? _currentFilePath = null;

    public IReadOnlyList<RecordedEvent> EventSequence => _eventSequence.AsReadOnly();
    public AppState CurrentState { get; private set; } = AppState.Idle;
    public string? CurrentFilePath => _currentFilePath;

    public event Action<AppState>? StateChanged;
    public event Action EventSequenceChanged;
    public event Action<string> StatusMessageChanged;

    public MacroController()
    {
        _recordingService = new RecordingService();
        _recordingService.OnEventRecorded += (ev) =>
        {
            _eventSequence.Add(ev);
            EventSequenceChanged?.Invoke();
        };

        // 使用策略模式初始化回放服务
        var playerStrategies = new Dictionary<Type, IEventPlayer>
        {
            { typeof(MouseEvent), new MouseEventPlayer() },
            { typeof(KeyboardEvent), new KeyboardEventPlayer() },
            { typeof(DelayEvent), new DelayEventPlayer() },
            { typeof(PixelConditionEvent), new PixelConditionEventPlayer() },
            { typeof(JumpEvent), new JumpEventPlayer() },
            { typeof(ConditionalJumpEvent), new ConditionalJumpEventPlayer() }
        };
        _playbackService = new PlaybackService(playerStrategies);
    }

    public void NewSequence()
    {
        _eventSequence.Clear();
        _currentFilePath = null;
        EventSequenceChanged?.Invoke();
        StatusMessageChanged?.Invoke("已创建新序列。");
    }

    public void LoadSequence(string filePath)
    {
        try
        {
            _eventSequence = FileService.Load(filePath);
            _currentFilePath = filePath;
            EventSequenceChanged?.Invoke();
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
            throw new InvalidOperationException("没有可保存的文件路径。请使用“另存为”。");
        }

        try
        {
            FileService.Save(pathToSave, _eventSequence);
            _currentFilePath = pathToSave;
            StatusMessageChanged?.Invoke($"文件已保存到 {pathToSave}");
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"保存文件失败: {ex.Message}", ex);
        }
    }

    public void AddEvent(RecordedEvent ev)
    {
        _eventSequence.Add(ev);
        EventSequenceChanged?.Invoke();
    }

    public void DeleteEventsAt(IEnumerable<int> indices)
    {
        var sortedIndices = new List<int>(indices);
        sortedIndices.Sort();
        for (int i = sortedIndices.Count - 1; i >= 0; i--)
        {
            _eventSequence.RemoveAt(sortedIndices[i]);
        }
        EventSequenceChanged?.Invoke();
    }

    public void StartRecording()
    {
        if (CurrentState != AppState.Idle) return;
        _eventSequence.Clear();
        EventSequenceChanged?.Invoke();
        _recordingService.Start();
        SetState(AppState.Recording);
        StatusMessageChanged?.Invoke("录制中... 按 F11 停止。");
    }

    public async Task StartPlayback()
    {
        if (CurrentState != AppState.Idle) return;

        SetState(AppState.Playing);
        StatusMessageChanged?.Invoke("播放中... 按 F11 停止。");
        try
        {
            await _playbackService.Play(_eventSequence, async (filePath) =>
            {
                StatusMessageChanged?.Invoke($"跳转到文件: {filePath} 并开始播放...");
                var newSequence = FileService.Load(filePath);
                await _playbackService.Play(newSequence, null); // 嵌套播放暂时不支持再次跳转
            });
            StatusMessageChanged?.Invoke("播放完成。");
        }
        catch (TaskCanceledException)
        {
            StatusMessageChanged?.Invoke("播放已停止。");
        }
        finally
        {
            SetState(AppState.Idle);
        }
    }

    public void Stop()
    {
        if (CurrentState == AppState.Recording)
        {
            _recordingService.Stop();
            StatusMessageChanged?.Invoke("录制结束。");
        }
        else if (CurrentState == AppState.Playing)
        {
            _playbackService.Stop();
            // 状态将在 StartPlayback 的 finally 块中被设置
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
