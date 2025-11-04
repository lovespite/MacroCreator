/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// 表示 CH9329 通信期间发生的异常。
/// </summary>
public class CH9329Exception : Exception
{
    /// <summary>
    /// 导致异常的错误代码。
    /// </summary>
    public Ch9329Error ErrorCode { get; }

    public CH9329Exception(Ch9329Error errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}