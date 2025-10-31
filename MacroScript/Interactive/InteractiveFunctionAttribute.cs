namespace MacroScript.Interactive;

[AttributeUsage(AttributeTargets.Method)]
public class InteractiveFunctionAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}