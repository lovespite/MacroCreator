/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

using MacroCreator.Models;

namespace MacroCreator.Models;

/// <summary>
/// 标准键盘控制键 (Bit flags)。
/// </summary>
[Flags]
public enum KeyModifier : byte
{
    None = 0,
    LeftCtrl = 1 << 0,
    LeftShift = 1 << 1,
    LeftAlt = 1 << 2,
    LeftWindows = 1 << 3,
    RightCtrl = 1 << 4,
    RightShift = 1 << 5,
    RightAlt = 1 << 6,
    RightWindows = 1 << 7
}


public static class KeyModifierExtensions
{
    /// <summary>
    /// 将 KeyModifier 转换为对应的字节值。
    /// </summary>
    public static byte ToByte(this KeyModifier modifier)
    {
        return (byte)modifier;
    }

    public static Keys[] ToKeysArray(this KeyModifier modifier)
    {
        var keys = new List<Keys>();
        if (modifier.HasFlag(KeyModifier.LeftCtrl)) keys.Add(Keys.LControlKey);
        if (modifier.HasFlag(KeyModifier.LeftShift)) keys.Add(Keys.LShiftKey);
        if (modifier.HasFlag(KeyModifier.LeftAlt)) keys.Add(Keys.LMenu);
        if (modifier.HasFlag(KeyModifier.LeftWindows)) keys.Add(Keys.LWin);
        if (modifier.HasFlag(KeyModifier.RightCtrl)) keys.Add(Keys.RControlKey);
        if (modifier.HasFlag(KeyModifier.RightShift)) keys.Add(Keys.RShiftKey);
        if (modifier.HasFlag(KeyModifier.RightAlt)) keys.Add(Keys.RMenu);
        if (modifier.HasFlag(KeyModifier.RightWindows)) keys.Add(Keys.RWin);
        return [.. keys];
    }
}