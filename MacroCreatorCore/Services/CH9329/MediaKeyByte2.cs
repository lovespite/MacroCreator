/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// 多媒体键 (Byte 2)。
/// </summary>
[Flags]
public enum MediaKeyByte2 : byte
{
    None = 0,
    VolumeUp = 1 << 0,
    VolumeDown = 1 << 1,
    Mute = 1 << 2,
    PlayPause = 1 << 3,
    NextTrack = 1 << 4,
    PrevTrack = 1 << 5,
    Stop = 1 << 6,
    Eject = 1 << 7
}
 