// 命名空间定义了应用程序的入口点
using MacroCreator.Models.Events;

namespace MacroCreator.Services;

/// <summary>
/// 播放上下文，包含播放过程中需要的共享状态
/// </summary>
public class PlaybackContext : IDisposable
{
    private readonly Dictionary<string, int> _indexCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object?> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<MacroEvent, ConditionEvaluator> _exprCache = [];
    private readonly DynamicExpresso.Interpreter _interpreter = new();
    private readonly CancellationTokenSource _cts = new();

    private readonly IPrintService? _printer;

    public int CurrentEventIndex { get; private set; } = 0;

    public MacroEvent CurrentEvent => Events[CurrentEventIndex];

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
    /// 事件序列的只读列表
    /// </summary>
    public IReadOnlyList<MacroEvent> Events { get; }

    public PlaybackContext(IReadOnlyList<MacroEvent> events, CallExternalFileDelegate? callback = null, IPrintService? printer = null)
    {
        Events = events;
        LoadAndPlayNewFileCallback = callback;

        _interpreter = CreateInterpreter();
        _printer = printer;

        BuildEventNameIndexCache();
    }

    public PlaybackContext() : this([], null, null)
    {
    }

    public DynamicExpresso.Interpreter CreateInterpreter()
    {
        PlaybackContext context = this;
        var interpreter = new DynamicExpresso.Interpreter();

        interpreter.SetVariable("runtime", context);

        interpreter.SetFunction("set", context.Set);
        interpreter.SetFunction("get", context.Get);
        interpreter.SetFunction("unset", context.Unset);
        interpreter.SetFunction("now", () => DateTime.Now);
        interpreter.SetFunction("utcnow", () => DateTime.UtcNow);
        interpreter.SetFunction("print", Print);
        interpreter.SetFunction("println", PrintLine);

        return interpreter;
    }

    private void Print(object? message)
    {
        _printer?.Print(message);
    }

    private void PrintLine(object? message)
    {
        _printer?.PrintLine(message);
    }

    private void BuildEventNameIndexCache()
    {
        _indexCache.Clear();
        for (int i = 0; i < Events.Count; i++)
        {
            var eventName = Events[i].EventName;
            if (!string.IsNullOrWhiteSpace(eventName) && !_indexCache.ContainsKey(eventName))
            {
                _indexCache[eventName] = i;
            }
        }
    }

    public bool MoveNext()
    {
        if (CurrentEventIndex + 1 < Events.Count)
        {
            CurrentEventIndex++;
            return true;
        }

        return false;
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

    /// <summary>
    /// 根据事件名称查找事件索引
    /// </summary>
    /// <param name="eventName">要查找的事件名称</param>
    /// <returns>事件索引（从0开始），如果未找到返回-1</returns>
    public int FindEventIndexByName(string? eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName)) return -1;
        if (_indexCache.TryGetValue(eventName, out int index)) return index;
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

    public ConditionEvaluator GetConditionEvaluator(ConditionalJumpEvent @event)
    {
        if (_exprCache.TryGetValue(@event, out var func))
            return func;

        var expr = _interpreter.ParseAsDelegate<ConditionEvaluator>(@event.CustomCondition)
            ?? throw new InvalidOperationException($"无法编译条件表达式: `{@event.CustomCondition}`");

        return (_exprCache[@event] = expr);
    }

    public object Execute(string expression)
    {
        return _interpreter.Eval(expression);
    }

    public void Set(string name, object? value)
    {
        _variables[name] = value;
    }

    public void Unset(string name)
    {
        _variables.Remove(name);
    }

    public object? Get(string name)
    {
        _variables.TryGetValue(name, out var value);
        return value;
    }
}

public delegate Task CallExternalFileDelegate(string filePath);
public delegate bool ConditionEvaluator();