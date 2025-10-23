using MacroCreator.Utils;

// 命名空间定义了应用程序的入口点
namespace MacroCreator.Models.Events;

public class ScriptEvent : MacroEvent
{
    public string Expression { get; set; } = string.Empty;
    
    public override string GetDescription()
    {
        return $"Script: {Expression.Ellipsize(25)}";
    }

    public override string TypeName => "Script";
}