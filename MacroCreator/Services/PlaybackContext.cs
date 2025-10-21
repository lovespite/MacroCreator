// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// 播放上下文，包含播放过程中需要的共享状态
/// </summary>
public class PlaybackContext : IDisposable
{
    public int CurrentEventIndex { get; private set; } = 0;
    public RecordedEvent CurrentEvent => Events[CurrentEventIndex];

    private readonly CancellationTokenSource _cts = new();

    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// 取消令牌，用于停止播放
    /// </summary> 
    public CancellationToken CancellationToken => _cts.Token;

    /// <summary>
    /// 加载并播放新文件的回调函数
    /// </summary>
    public CallExternalFileDelegate? LoadAndPlayNewFileCallback { get; }

    /// <summary>
    /// 事件序列的只读列表，用于按名称查找事件
    /// </summary>
    public IReadOnlyList<RecordedEvent> Events { get; }

    private readonly Dictionary<string, int> _eventNameToIndexCache = new(StringComparer.OrdinalIgnoreCase);

    public PlaybackContext(IReadOnlyList<RecordedEvent> events, CallExternalFileDelegate? callback)
    {
        Events = events;
        LoadAndPlayNewFileCallback = callback;

        BuildEventNameIndexCache();
    }

    public bool MoveNext()
    {
        if (CurrentEventIndex + 1 < Events.Count)
        {
            CurrentEventIndex++;
            return true;
        }
        else
        {
            return false;
        }

    }

    public int MoveTo(int index)
    {
        if (index < 0 || index >= Events.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "事件索引超出序列范围");
        }
        CurrentEventIndex = index;
        return CurrentEventIndex;
    }

    private void BuildEventNameIndexCache()
    {
        _eventNameToIndexCache.Clear();
        for (int i = 0; i < Events.Count; i++)
        {
            var eventName = Events[i].EventName;
            if (!string.IsNullOrWhiteSpace(eventName) && !_eventNameToIndexCache.ContainsKey(eventName))
            {
                _eventNameToIndexCache[eventName] = i;
            }
        }
    }

    /// <summary>
    /// 根据事件名称查找事件索引
    /// </summary>
    /// <param name="eventName">要查找的事件名称</param>
    /// <returns>事件索引（从0开始），如果未找到返回-1</returns>
    public int FindEventIndexByName(string? eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            return -1;
        }
        if (_eventNameToIndexCache.TryGetValue(eventName, out int index))
        {
            return index;
        }
        return -1;
    }

    public void Abort()
    {
        if (IsDisposed) return;
        _cts.Cancel();
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}

public delegate Task CallExternalFileDelegate(string filePath);