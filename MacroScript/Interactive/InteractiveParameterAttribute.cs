namespace MacroScript.Interactive;

public delegate object? ParamterConverter(string parameterExpr);

[AttributeUsage(AttributeTargets.Parameter)]
public class InteractiveParameterAttribute : Attribute
{
    public string? Description { get; set; }
    public ParamterConverter? Converter { get; set; }
}
