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
[XmlInclude(typeof(JumpEvent))]
[XmlInclude(typeof(ConditionalJumpEvent))]
[Serializable]
public abstract class MacroEvent
{
    /// <summary>
    /// 与上一个事件的时间间隔（毫秒，支持小数以提高精度）
    /// </summary>
    public double TimeSinceLastEvent { get; set; }

    /// <summary>
    /// 事件的绝对时间戳（毫秒）
    /// </summary>
    [XmlIgnore]
    public double Timestamp { get; set; }

    /// <summary>
    /// 事件名称（可选），用于标识和跳转。默认为 null（匿名事件） 
    /// </summary>
    public string? EventName { get; set; }

    /// <summary>
    /// 指示事件是否有有效名称
    /// </summary>
    public bool HasName => !string.IsNullOrWhiteSpace(EventName);

    /// <summary>
    /// 验证事件名称是否有效
    /// </summary>
    public static bool IsValidEventName(string? name)
    {
        //if (string.IsNullOrWhiteSpace(name))
        //    return false;

        //foreach (char c in name)
        //{
        //    if (!char.IsLetterOrDigit(c) || c > 127) // 只允许ASCII字母和数字
        //        return false;
        //} 

        return !string.IsNullOrWhiteSpace(name);
    }

    public virtual string TypeName => GetType().Name;

    public virtual string DisplayName => EventName is null ? TypeName : EventName;

    public abstract string GetDescription();
}

public delegate bool ContainsEventNameDelegate(string eventName);