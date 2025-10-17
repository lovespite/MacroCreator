using System.Xml.Serialization;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 所有录制事件的基类
/// XmlInclude 属性是必需的，以便 XmlSerializer 能够识别和处理派生类。
/// </summary>
[XmlInclude(typeof(MouseEvent))]
[XmlInclude(typeof(KeyboardEvent))]
[XmlInclude(typeof(DelayEvent))]
[XmlInclude(typeof(PixelConditionEvent))]
[XmlInclude(typeof(JumpEvent))]
[XmlInclude(typeof(ConditionalJumpEvent))]
[Serializable]
public abstract class RecordedEvent
{
    /// <summary>
    /// 与上一个事件的时间间隔（毫秒，支持小数以提高精度）
    /// </summary>
    public double TimeSinceLastEvent { get; set; }
    
    /// <summary>
    /// 事件的绝对时间戳（微秒），用于更精确的时间计算
    /// </summary>
    [XmlIgnore]
    public long AbsoluteTimestamp { get; set; }
    
    /// <summary>
    /// 事件优先级，用于在时间紧张时调整执行顺序
    /// </summary>
    public EventPriority Priority { get; set; } = EventPriority.Normal;
    
    public abstract string GetDescription();
}

/// <summary>
/// 事件优先级枚举
/// </summary>
public enum EventPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

