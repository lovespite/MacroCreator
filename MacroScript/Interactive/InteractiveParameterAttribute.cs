namespace MacroScript.Interactive;

public delegate object? ParamterDeserializer(string parameterExpr);

[AttributeUsage(AttributeTargets.Parameter)]
public class InteractiveParameterAttribute : Attribute
{
    public string? Description { get; set; }
}
