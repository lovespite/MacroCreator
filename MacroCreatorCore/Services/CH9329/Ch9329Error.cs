/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// 芯片返回的错误状态码。
/// </summary>
public enum Ch9329Error : byte
{
    /// <summary>
    /// (0x00) 命令执行成功。
    /// </summary>
    Success = 0x00,
    /// <summary>
    /// (0xE1) 串口接收一个字节超时。
    /// </summary>
    CmdErrTimeout = 0xE1,
    /// <summary>
    /// (0xE2) 串口接收包头字节出错。
    /// </summary>
    CmdErrHead = 0xE2,
    /// <summary>
    /// (0xE3) 串口接收命令码错误。
    /// </summary>
    CmdErrCmd = 0xE3,
    /// <summary>
    /// (0xE4) 累加和检验值不匹配。
    /// </summary>
    CmdErrSum = 0xE4,
    /// <summary>
    /// (0xE5) 参数错误。
    /// </summary>
    CmdErrPara = 0xE5,
    /// <summary>
    /// (0xE6) 帧正常，执行失败。
    /// </summary>
    CmdErrOperate = 0xE6,
    /// <summary>
    /// (0xFF) 内部协议错误 (非芯片定义)。
    /// </summary>
    ProtocolError = 0xFF
}
 