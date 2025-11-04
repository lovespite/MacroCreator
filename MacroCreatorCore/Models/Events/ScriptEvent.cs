using MacroCreator.Utils;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models.Events;

public class ScriptEvent : MacroEvent
{
    public string[] ScriptLines { get; set; } = [];

    public override string GetDescription()
    {
        if (ScriptLines is null || ScriptLines.Length == 0)
            return "Script: (empty)";

        return $"Script: {ScriptLines[0].Ellipsize(25)}";
    }

    public override string TypeName => "Script";
}