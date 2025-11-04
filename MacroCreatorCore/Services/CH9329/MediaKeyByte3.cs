/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// 多媒体键 (Byte 3)。
/// </summary>
[Flags]
public enum MediaKeyByte3 : byte
{
    None = 0,
    Email = 1 << 0,
    Search = 1 << 1,
    Favorites = 1 << 2,
    Home = 1 << 3,
    Back = 1 << 4,
    Forward = 1 << 5,
    StopWWW = 1 << 6,
    Refresh = 1 << 7
}
 