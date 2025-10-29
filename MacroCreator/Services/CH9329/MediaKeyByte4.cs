/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// 多媒体键 (Byte 4)。
/// </summary>
[Flags]
public enum MediaKeyByte4 : byte
{
    None = 0,
    Media = 1 << 0,
    Explorer = 1 << 1,
    Calculator = 1 << 2,
    ScreenSave = 1 << 3,
    MyComputer = 1 << 4,
    Minimize = 1 << 5,
    Record = 1 << 6,
    Rewind = 1 << 7
}
 