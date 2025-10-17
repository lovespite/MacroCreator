using MacroCreator.Models;
using System.Xml.Serialization;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Services;

/// <summary>
/// 负责处理文件的加载和保存
/// </summary>
public static class FileService
{
    public static void Save(string filePath, List<RecordedEvent> events)
    {
        XmlSerializer serializer = new(typeof(List<RecordedEvent>));
        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, events);
    }

    public static List<RecordedEvent> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("文件未找到。", filePath);
        }
        XmlSerializer serializer = new(typeof(List<RecordedEvent>));
        using var reader = new StreamReader(filePath);

        return (List<RecordedEvent>)serializer.Deserialize(reader);
    }
}

