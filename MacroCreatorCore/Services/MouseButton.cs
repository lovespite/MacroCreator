namespace MacroCreator.Services;

/// <summary>
/// 鼠标按键 (Bit flags)。
/// </summary>
[Flags]
public enum MouseButton : byte
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Middle = 1 << 2
}
