// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

/// <summary>
/// 播放上下文，包含播放过程中需要的共享状态
/// </summary>
public class PlaybackContext
{
    /// <summary>
    /// 取消令牌，用于停止播放
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// 加载并播放新文件的回调函数
    /// </summary>
    public Func<string, Task>? LoadAndPlayNewFileCallback { get; }

    /// <summary>
    /// 事件序列的只读列表，用于按名称查找事件
    /// </summary>
    public IReadOnlyList<RecordedEvent> Events { get; }

    private readonly IDictionary<string, int> _eventNameToIndexCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public PlaybackContext(CancellationToken token, IReadOnlyList<RecordedEvent> events, Func<string, Task>? callback)
    {
        CancellationToken = token;
        Events = events;
        LoadAndPlayNewFileCallback = callback;

        BuildEventNameIndexCache();
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
}
