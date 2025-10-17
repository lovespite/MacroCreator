// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models;

/// <summary>
/// 条件判断事件
/// </summary>
public class PixelConditionEvent : RecordedEvent
{
    public int X { get; set; }
    public int Y { get; set; }
    public string ExpectedColorHex { get; set; }
    public string FilePathIfMatch { get; set; }
    public string FilePathIfNotMatch { get; set; }

    public override string GetDescription()
    {
        return $"条件判断: 检查点({X},{Y})颜色是否为{ExpectedColorHex}。";
    }
} 

