/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// USB 枚举状态。
/// </summary>
public enum UsbStatus : byte
{
    /// <summary>
    /// USB 端未连接计算机或未识别。
    /// </summary>
    NotConnected = 0x00,
    /// <summary>
    /// USB 端已连接计算机并识别成功。
    /// </summary>
    ConnectedAndRecognized = 0x01
}
 