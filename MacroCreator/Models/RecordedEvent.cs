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
[Serializable]
public abstract class RecordedEvent
{
    public long TimeSinceLastEvent { get; set; }
    public abstract string GetDescription();
}

