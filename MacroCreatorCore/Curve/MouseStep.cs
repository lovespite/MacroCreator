namespace MacroCreator.Curve;

/// <summary>
/// 表示鼠标移动的一个单独步骤
/// </summary>
public struct MouseStep
{
    /// <summary>
    /// X方向的偏移量
    /// </summary>
    public int DeltaX;

    /// <summary>
    /// Y方向的偏移量
    /// </summary>
    public int DeltaY;

    /// <summary>
    /// 执行此步骤后应等待的毫秒数
    /// </summary>
    public int DelayMs;

    public override string ToString()
    {
        return $"Move ({DeltaX}, {DeltaY}) over {DelayMs}ms";
    }
}
