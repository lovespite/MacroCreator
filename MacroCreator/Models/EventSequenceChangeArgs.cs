namespace MacroCreator.Models;

/// <summary>
/// 事件序列变更类型
/// </summary>
public enum EventSequenceChangeType
{
    /// <summary>
    /// 添加单个事件（在末尾）
    /// </summary>
    Add,

    /// <summary>
    /// 在指定位置插入单个事件
    /// </summary>
    Insert,

    /// <summary>
    /// 删除单个或多个事件
    /// </summary>
    Delete,

    /// <summary>
    /// 替换单个事件
    /// </summary>
    Replace,

    /// <summary>
    /// 更新单个事件的属性（不改变位置）
    /// </summary>
    Update,

    /// <summary>
    /// 清空所有事件
    /// </summary>
    Clear,

    /// <summary>
    /// 需要完全刷新（如加载文件）
    /// </summary>
    FullRefresh
}

/// <summary>
/// 事件序列变更参数
/// </summary>
public class EventSequenceChangeArgs
{
    /// <summary>
    /// 变更类型
    /// </summary>
    public EventSequenceChangeType ChangeType { get; set; }

    /// <summary>
    /// 受影响的索引（单个操作）或起始索引（多个操作）
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// 受影响的索引列表（用于批量删除等操作）
    /// </summary>
    public List<int>? Indices { get; set; }

    /// <summary>
    /// 相关的事件对象
    /// </summary>
    public MacroEvent? Event { get; set; }

    public EventSequenceChangeArgs(EventSequenceChangeType changeType, int? index = null, MacroEvent? ev = null)
    {
        ChangeType = changeType;
        Index = index;
        Event = ev;
    }

    public EventSequenceChangeArgs(EventSequenceChangeType changeType, List<int> indices)
    {
        ChangeType = changeType;
        Indices = indices;
    }
}
