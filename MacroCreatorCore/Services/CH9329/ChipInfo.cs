/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// (CMD_GET_INFO) 芯片信息响应。
/// </summary>
public readonly struct ChipInfo(byte version, UsbStatus usbStatus, KeyboardLedStatus ledStatus)
{
    /// <summary>
    /// 芯片版本号。例如 0x30 表示 V1.0。
    /// </summary>
    public readonly byte Version = version;
    /// <summary>
    /// USB 枚举状态。
    /// </summary>
    public readonly UsbStatus UsbStatus = usbStatus;
    /// <summary>
    /// 键盘大小写指示灯状态。
    /// </summary>
    public readonly KeyboardLedStatus LedStatus = ledStatus;

    public string GetVersionString()
    {
        // 0x30 -> "1.0", 0x31 -> "1.1"
        if (Version >= 0x30 && Version <= 0x39)
        {
            return $"1.{(Version & 0x0F)}";
        }
        if (Version >= 0x10 && Version <= 0x99)
        {
            return $"{(Version >> 4)}.{Version & 0x0F}";
        }
        return $"0x{Version:X2}";
    }

    public string GetConnectionString()
    {
        return UsbStatus switch
        {
            UsbStatus.NotConnected => "未连接",
            UsbStatus.ConnectedAndRecognized => "已连接并识别",
            _ => "未知状态",
        };
    }

    public string GetLedStatusString()
    {
        var status = $"Capslock: {(LedStatus.HasFlag(KeyboardLedStatus.CapsLock) ? "ON" : "OFF")}";
        status += $", Numlock: {(LedStatus.HasFlag(KeyboardLedStatus.NumLock) ? "ON" : "OFF")}";
        status += $", Scrolllock: {(LedStatus.HasFlag(KeyboardLedStatus.ScrollLock) ? "ON" : "OFF")}";

        return status;
    }

    public override string ToString()
    {
        return $"芯片版本: {GetVersionString()}, 连接状态: {GetConnectionString()}, 指示灯: {GetLedStatusString()}";
    }
}
