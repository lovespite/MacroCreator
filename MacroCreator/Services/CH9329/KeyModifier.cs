/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

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
 