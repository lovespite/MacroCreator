/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// 键盘指示灯状态 (Bit flags)。
/// </summary>
[Flags]
public enum KeyboardLedStatus : byte
{
    None = 0,
    /// <summary>
    /// 键盘 NUM LOCK 指示灯。
    /// </summary>
    NumLock = 1 << 0,
    /// <summary>
    /// 键盘 CAPS LOCK 指示灯。
    /// </summary>
    CapsLock = 1 << 1,
    /// <summary>
    /// 键盘 SCROLL LOCK 指示灯。
    /// </summary>
    ScrollLock = 1 << 2
}
 