/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

using System.Buffers;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using MacroCreator.Models;
using MacroCreator.Models;

namespace MacroCreator.Services.CH9329;

/// <summary>
/// CH9329 芯片串口通信控制器。
/// 性能优化版本：使用 ArrayPool 池化缓冲区、Span<T> 优化内存操作、减少内存分配。
/// </summary>
public class Ch9329Controller : IDisposable
{
    private readonly byte _chipAddress;
    private readonly SerialPort _serialPort;
    private readonly SemaphoreSlim _lock = new(1, 1);

    // 使用 ArrayPool 来减少内存分配
    private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

    // 缓存常用的键盘释放命令帧（减少重复构建）
    private byte[]? _cachedKeyReleaseFrame;
    private byte[]? _cachedMouseReleaseFrame;

    private const byte FRAME_HEAD_1 = 0x57;
    private const byte FRAME_HEAD_2 = 0xAB;
    private const int DEFAULT_TIMEOUT = 500; // 默认超时时间 (ms)
    private const int MAX_FRAME_SIZE = 70; // 最大帧大小 (头部6字节 + 数据64字节)

    #region 命令码 (Command Codes)
    private const byte CMD_GET_INFO = 0x01;
    private const byte CMD_SEND_KB_GENERAL_DATA = 0x02;
    private const byte CMD_SEND_KB_MEDIA_DATA = 0x03;
    private const byte CMD_SEND_MS_ABS_DATA = 0x04;
    private const byte CMD_SEND_MS_REL_DATA = 0x05;
    private const byte CMD_SEND_MY_HID_DATA = 0x06;
    // 0x87 (CMD_READ_MY_HID_DATA) 是芯片主动发送的，不在此处作为命令
    private const byte CMD_GET_PARA_CFG = 0x08;
    private const byte CMD_SET_PARA_CFG = 0x09;
    private const byte CMD_GET_USB_STRING = 0x0A;
    private const byte CMD_SET_USB_STRING = 0x0B;
    private const byte CMD_SET_DEFAULT_CFG = 0x0C;
    private const byte CMD_RESET = 0x0F;

    private const byte RSP_SUCCESS_FLAG = 0x80;
    private const byte RSP_ERROR_FLAG = 0xC0;
    #endregion

    /// <summary>
    /// 初始化 CH9329Controller 的一个新实例。
    /// </summary>
    /// <param name="portName">串口名称 (例如 "COM3")</param>
    /// <param name="baudRate">波特率 (默认为 9600)</param>
    /// <param name="chipAddress">芯片地址 (默认为 0x00)</param>
    public Ch9329Controller(string portName, int baudRate = 9600, byte chipAddress = 0x00)
    {
        _chipAddress = chipAddress;
        _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout = DEFAULT_TIMEOUT,
            WriteTimeout = DEFAULT_TIMEOUT
        };
    }

    /// <summary>
    /// 端口名称
    /// </summary>
    public string PortName => _serialPort.PortName;

    /// <summary>
    /// 打开串口连接。
    /// </summary>
    public void Open()
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.Open();
        }
    }

    /// <summary>
    /// 关闭串口连接。
    /// </summary>
    public void Close()
    {

#if DEBUG
        try
        {
            Debug.WriteLine("正在关闭串口");
            _serialPort.Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("关闭串口发生异常: " + ex);
        }
#else
        try
        { 
            _serialPort.Close();
        }
        catch 
        { 
            // ignore
        }
#endif
    }

    #region 公共命令 (Public Commands)

    /// <summary>
    /// (CMD_GET_INFO) 异步获取芯片版本号、USB枚举状态和键盘指示灯状态。
    /// </summary>
    /// <returns>包含芯片信息的 <see cref="ChipInfo"/> 对象。</returns>
    public async Task<ChipInfo> GetInfoAsync()
    {
        byte[] response = await SendCommandAsync(CMD_GET_INFO);

        // 预期响应数据长度为 8
        if (response.Length != 8)
        {
            throw new CH9329Exception(Ch9329Error.ProtocolError, $"GetInfoAsync 期望 8 字节数据，但收到了 {response.Length} 字节。");
        }

        return new ChipInfo(
            version: response[0],
            usbStatus: (UsbStatus)response[1],
            ledStatus: (KeyboardLedStatus)response[2]
        );
    }

    /// <summary>
    /// (CMD_SEND_KB_GENERAL_DATA) 异步发送标准键盘数据（按下或释放）。
    /// </summary>
    /// <param name="modifiers">控制键 (例如 Ctrl, Shift)。使用 <see cref="KeyModifier"/> 枚举。</param>
    /// <param name="keys">6个普通按键的键码。如果少于6个，其余用 0x00 填充。全部为 0x00 表示释放按键。</param>
    public async Task SendKeyboardDataAsync(KeyModifier modifiers, params byte[] keys)
    {
        if (keys == null) keys = [];
        if (keys.Length > 6) throw new ArgumentException("最多只能指定 6 个普通按键。", nameof(keys));

        // 检查是否是键盘释放命令（全零），使用缓存的帧
        if (modifiers == 0 && keys.Length == 0)
        {
            await SendCachedKeyReleaseAsync();
            return;
        }

        // 使用池化数组
        byte[] data = _bufferPool.Rent(8);
        try
        {
            data[0] = (byte)modifiers;
            data[1] = 0x00; // 保留字节

            // 填充按键
            for (int i = 0; i < 6; i++)
            {
                data[i + 2] = (i < keys.Length) ? keys[i] : (byte)0x00;
            }

            byte[] response = await SendCommandAsync(CMD_SEND_KB_GENERAL_DATA, data.AsMemory(0, 8));
            ValidateSimpleResponse(response, "SendKeyboardDataAsync");
        }
        finally
        {
            _bufferPool.Return(data);
        }
    }

    /// <summary>
    /// 发送缓存的键盘释放命令（性能优化）。
    /// </summary>
    private async Task SendCachedKeyReleaseAsync()
    {
        if (_cachedKeyReleaseFrame == null)
        {
            byte[] data = new byte[8]; // 全零数据
            _cachedKeyReleaseFrame = BuildFrame(CMD_SEND_KB_GENERAL_DATA, data);
        }

        byte[] response = await SendFrameDirectAsync(_cachedKeyReleaseFrame, CMD_SEND_KB_GENERAL_DATA);
        ValidateSimpleResponse(response, "SendKeyboardDataAsync");
    }

    /// <summary>
    /// (CMD_SEND_KB_GENERAL_DATA) 异步发送标准键盘数据（按下或释放）。
    /// 此重载使用 .NET Keys 枚举。
    /// </summary>
    /// <param name="modifiers">控制键 (例如 Ctrl, Shift)。使用 <see cref="KeyModifier"/> 枚举。</param>
    /// <param name="keys">最多 6 个普通按键。修饰键 (Ctrl, Shift, Alt, Win) 应通过 'modifiers' 参数传递。</param>
    public async Task SendKeyboardDataAsync(KeyModifier modifiers, params Keys[] keys)
    {
        // 调用原始方法，它会处理填充到 6 个字节
        await SendKeyboardDataAsync(modifiers, keys.Select(HidKeys.Map).ToArray());
    }

    /// <summary>
    /// (CMD_SEND_KB_MEDIA_DATA) 异步发送多媒体键盘数据（按下或释放）。
    /// 注意：根据协议文档示例，数据长度为 4 字节。
    /// </summary>
    /// <param name="reportId">Report ID (固定为 0x02)</param>
    /// <param name="byte2">多媒体键 (例如 <see cref="MediaKeyByte2"/>)</param>
    /// <param name="byte3">多媒体键 (例如 <see cref="MediaKeyByte3"/>)</param>
    /// <param name="byte4">多媒体键 (例如 <see cref="MediaKeyByte4"/>)</param>
    public async Task SendMediaKeyAsync(byte reportId, byte byte2, byte byte3, byte byte4)
    {
        if (reportId != 0x02)
        {
            // 协议中 ACPI 键 (0x01) 格式不同，此处仅支持多媒体键 (0x02)
            throw new ArgumentException("此方法仅支持 Report ID 0x02 的多媒体键。", nameof(reportId));
        }

        byte[] data = [0x02, byte2, byte3, byte4];
        byte[] response = await SendCommandAsync(CMD_SEND_KB_MEDIA_DATA, data);
        ValidateSimpleResponse(response);
    }

    /// <summary>
    /// (CMD_SEND_MS_ABS_DATA) 异步发送绝对鼠标数据（移动、点击、滚轮）。
    /// </summary>
    /// <param name="buttons">鼠标按键 (例如 <see cref="MouseButton"/>)。</param>
    /// <param name="x">X轴绝对坐标 (0-4095)。</param>
    /// <param name="y">Y轴绝对坐标 (0-4095)。</param>
    /// <param name="wheel">滚轮滚动。正值向上，负值向下。</param>
    public async Task SendAbsoluteMouseAsync(MouseButton buttons, ushort x, ushort y, sbyte wheel)
    {
        if (x > 4095) x = 4095;
        if (y > 4095) y = 4095;

        byte[] data = _bufferPool.Rent(7);
        try
        {
            data[0] = 0x02; // 固定值
            data[1] = (byte)buttons;
            data[2] = (byte)(x & 0xFF);         // X 坐标低字节
            data[3] = (byte)((x >> 8) & 0xFF); // X 坐标高字节
            data[4] = (byte)(y & 0xFF);         // Y 坐标低字节
            data[5] = (byte)((y >> 8) & 0xFF); // Y 坐标高字节
            data[6] = (byte)wheel;

            byte[] response = await SendCommandAsync(CMD_SEND_MS_ABS_DATA, data.AsMemory(0, 7));
            ValidateSimpleResponse(response);
        }
        finally
        {
            _bufferPool.Return(data);
        }
    }

    /// <summary>
    /// (CMD_SEND_MS_REL_DATA) 异步发送相对鼠标数据（移动、点击、滚轮）。
    /// </summary>
    /// <param name="buttons">鼠标按键 (例如 <see cref="MouseButton"/>)。</param>
    /// <param name="dx">X轴相对移动 (-127 到 +127)。</param>
    /// <param name="dy">Y轴相对移动 (-127 到 +127)。</param>
    /// <param name="wheel">滚轮滚动。正值向上，负值向下。</param>
    public async Task SendRelativeMouseAsync(MouseButton buttons, sbyte dx, sbyte dy, sbyte wheel)
    {
        byte[] data = _bufferPool.Rent(5);
        try
        {
            data[0] = 0x01; // 固定值
            data[1] = (byte)buttons;
            data[2] = (byte)dx;
            data[3] = (byte)dy;
            data[4] = (byte)wheel;

            byte[] response = await SendCommandAsync(CMD_SEND_MS_REL_DATA, data.AsMemory(0, 5));
            ValidateSimpleResponse(response);
        }
        finally
        {
            _bufferPool.Return(data);
        }
    }

    /// <summary>
    /// (CMD_SEND_MY_HID_DATA) 异步发送自定义 HID 数据。
    /// </summary>
    /// <param name="hidData">要发送的 HID 数据 (最多 64 字节)。</param>
    public async Task SendCustomHidDataAsync(byte[] hidData)
    {
        if (hidData == null || hidData.Length == 0) throw new ArgumentNullException(nameof(hidData));
        if (hidData.Length > 64) throw new ArgumentException("HID 数据长度不能超过 64 字节。", nameof(hidData));

        byte[] response = await SendCommandAsync(CMD_SEND_MY_HID_DATA, hidData);
        ValidateSimpleResponse(response);
    }

    /// <summary>
    /// (CMD_GET_PARA_CFG) 异步获取芯片当前参数配置信息。
    /// </summary>
    /// <returns>包含 50 字节原始配置数据的字节数组。</returns>
    public async Task<byte[]> GetConfigAsync()
    {
        byte[] response = await SendCommandAsync(CMD_GET_PARA_CFG);
        if (response.Length != 50)
        {
            throw new CH9329Exception(Ch9329Error.ProtocolError, $"GetConfigAsync 期望 50 字节数据，但收到了 {response.Length} 字节。");
        }
        return response;
    }

    /// <summary>
    /// (CMD_SET_PARA_CFG) 异步设置芯片参数配置信息。
    /// </summary>
    /// <param name="configData">包含 50 字节配置数据的字节数组。</param>
    public async Task SetConfigAsync(byte[] configData)
    {
        if (configData == null || configData.Length != 50)
        {
            throw new ArgumentException("配置数据必须是 50 字节。", nameof(configData));
        }

        byte[] response = await SendCommandAsync(CMD_SET_PARA_CFG, configData);
        ValidateSimpleResponse(response);
    }

    /// <summary>
    /// (CMD_GET_USB_STRING) 异步获取 USB 字符串描述符。
    /// </summary>
    /// <param name="stringType">字符串类型 (0x00=厂商, 0x01=产品, 0x02=序列号)。</param>
    /// <returns>USB 字符串。</returns>
    public async Task<string> GetUsbStringAsync(byte stringType)
    {
        if (stringType > 0x02) throw new ArgumentException("无效的字符串类型。", nameof(stringType));

        byte[] data = [stringType];
        byte[] response = await SendCommandAsync(CMD_GET_USB_STRING, data);

        // 响应: [Type (1), Length (1), String (N)]
        if (response.Length < 2)
        {
            throw new CH9329Exception(Ch9329Error.ProtocolError, "GetUsbStringAsync 响应过短。");
        }

        byte type = response[0];
        byte len = response[1];
        if (type != stringType || response.Length != (len + 2))
        {
            throw new CH9329Exception(Ch9329Error.ProtocolError, "GetUsbStringAsync 响应数据格式或长度不匹配。");
        }

        // 假设字符串是 ASCII 编码
        return Encoding.ASCII.GetString(response, 2, len);
    }

    /// <summary>
    /// (CMD_SET_USB_STRING) 异步设置 USB 字符串描述符。
    /// </summary>
    /// <param name="stringType">字符串类型 (0x00=厂商, 0x01=产品, 0x02=序列号)。</param>
    /// <param name="descriptor">要设置的字符串 (最大 23 字节)。</param>
    public async Task SetUsbStringAsync(byte stringType, string descriptor)
    {
        if (stringType > 0x02) throw new ArgumentException("无效的字符串类型。", nameof(stringType));
        if (descriptor == null) descriptor = string.Empty;

        byte[] strBytes = Encoding.ASCII.GetBytes(descriptor);
        if (strBytes.Length > 23)
        {
            throw new ArgumentException("字符串描述符最大长度为 23 字节。", nameof(descriptor));
        }

        byte[] data = new byte[strBytes.Length + 2];
        data[0] = stringType;
        data[1] = (byte)strBytes.Length;
        Buffer.BlockCopy(strBytes, 0, data, 2, strBytes.Length);

        byte[] response = await SendCommandAsync(CMD_SET_USB_STRING, data);
        ValidateSimpleResponse(response);
    }

    /// <summary>
    /// (CMD_SET_DEFAULT_CFG) 异步恢复出厂默认配置。
    /// </summary>
    public async Task RestoreDefaultsAsync()
    {
        byte[] response = await SendCommandAsync(CMD_SET_DEFAULT_CFG);
        ValidateSimpleResponse(response);
    }

    /// <summary>
    /// (CMD_RESET) 异步复位芯片。
    /// </summary>
    public async Task ResetAsync()
    {
        byte[] response = await SendCommandAsync(CMD_RESET);
        ValidateSimpleResponse(response);
    }

    #endregion

    #region 私有辅助方法 (Private Helpers)

    /// <summary>
    /// 验证简单的成功/失败响应 (数据长度为 1，内容为 0x00)。
    /// </summary>
    private static void ValidateSimpleResponse(byte[] response, [CallerMemberName] string? commandName = null)
    {
        if (response.Length != 1 || response[0] != (byte)Ch9329Error.Success)
        {
            throw new CH9329Exception(
                (Ch9329Error)(response.Length > 0 ? response[0] : 0xFF),
                $"{commandName ?? string.Empty}执行失败或响应无效。状态码: 0x{(response.Length > 0 ? response[0] : 0xFF):X2}"
            );
        }
    }

    /// <summary>
    /// 直接构造帧并返回（用于缓存）。
    /// </summary>
    private byte[] BuildFrame(byte cmd, ReadOnlySpan<byte> data)
    {
        int dataLength = data.Length;
        if (dataLength > 64) throw new ArgumentException("数据长度不能超过 64 字节。");

        // 帧头(2) + 地址(1) + 命令(1) + 长度(1) + 数据(N) + 校验(1)
        Span<byte> frame = stackalloc byte[6 + dataLength];

        BuildFrameInternal(frame, cmd, data);

        return frame.ToArray();
    }

    /// <summary>
    /// 内部帧构造方法，将帧数据写入指定的缓冲区。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BuildFrameInternal(Span<byte> frame, byte cmd, ReadOnlySpan<byte> data)
    {
        int dataLength = data.Length;

        frame[0] = FRAME_HEAD_1;
        frame[1] = FRAME_HEAD_2;
        frame[2] = _chipAddress;
        frame[3] = cmd;
        frame[4] = (byte)dataLength;

        if (dataLength > 0)
        {
            data.CopyTo(frame[5..]);
        }

        // 计算校验和（使用向量化优化）
        byte checksum = CalculateChecksum(frame[..(5 + dataLength)]);
        frame[5 + dataLength] = checksum;
    }

    /// <summary>
    /// 计算校验和（性能优化版本）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte CalculateChecksum(ReadOnlySpan<byte> data)
    {
        int sum = 0;

        // 使用循环展开优化
        int i = 0;
        int length = data.Length;

        // 每次处理 4 个字节（循环展开）
        for (; i <= length - 4; i += 4)
        {
            sum += data[i];
            sum += data[i + 1];
            sum += data[i + 2];
            sum += data[i + 3];
        }

        // 处理剩余字节
        for (; i < length; i++)
        {
            sum += data[i];
        }

        return (byte)(sum & 0xFF);
    }

    /// <summary>
    /// 直接发送已构建的帧（用于缓存的帧）。
    /// </summary>
    private async Task<byte[]> SendFrameDirectAsync(byte[] frame, byte cmd)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException("串口未打开。");
            }

            _serialPort.DiscardInBuffer();
            await _serialPort.BaseStream.WriteAsync(frame, CancellationToken.None);
            return await ReadResponseAsync(cmd);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 异步发送命令并等待响应（性能优化版本）。
    /// </summary>
    private async Task<byte[]> SendCommandAsync(byte cmd, ReadOnlyMemory<byte> data)
    {
        await _lock.WaitAsync(); // 确保同一时间只有一个命令在执行
        try
        {
            if (!_serialPort.IsOpen) throw new InvalidOperationException("串口未打开。");

            _serialPort.DiscardInBuffer(); // 清空接收缓冲区

            // 使用池化缓冲区来构建帧
            int dataLength = data.Length;
            int frameSize = 6 + dataLength;
            byte[] frameBuffer = _bufferPool.Rent(frameSize);
            try
            {
                Span<byte> frame = frameBuffer.AsSpan(0, frameSize);
                BuildFrameInternal(frame, cmd, data.Span);

                // 发送命令
                await _serialPort.BaseStream.WriteAsync(frameBuffer.AsMemory(0, frameSize), CancellationToken.None);

                // 异步读取响应
                return await ReadResponseAsync(cmd);
            }
            finally
            {
                _bufferPool.Return(frameBuffer);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 异步发送命令并等待响应（性能优化版本）。
    /// </summary>
    private Task<byte[]> SendCommandAsync(byte cmd) => SendCommandAsync(cmd, ReadOnlyMemory<byte>.Empty);

    /// <summary>
    /// 异步读取和解析响应帧（性能优化版本）。
    /// </summary>
    private async Task<byte[]> ReadResponseAsync(byte originalCmd)
    {
        byte[] headerBuffer = _bufferPool.Rent(5); // HEAD(2) + ADDR(1) + CMD(1) + LEN(1)
        var mem = headerBuffer.AsMemory(0, 5);
        mem.Span.Clear();
        try
        {
            int bytesRead = 0;

            // 1. 读取帧头
            while (bytesRead < 5)
            {
                try
                {
                    bytesRead += await _serialPort.BaseStream.ReadAsync(mem[bytesRead..]);
                }
                catch (OperationCanceledException)
                {
                    throw new CH9329Exception(Ch9329Error.CmdErrTimeout, "读取响应头超时。");
                }
                catch (IOException)
                {
                    if (!_serialPort.IsOpen) throw;
                    // ignore
                }
                catch (Exception ex)
                {
                    throw new CH9329Exception(Ch9329Error.ProtocolError, $"串口读取错误: {ex.Message}");
                }
            }

            // 2. 验证帧头
            if (headerBuffer[0] != FRAME_HEAD_1 || headerBuffer[1] != FRAME_HEAD_2)
            {
                throw new CH9329Exception(Ch9329Error.CmdErrHead, "无效的响应帧头。");
            }

            // 3. 验证地址
            if (headerBuffer[2] != _chipAddress && _chipAddress != 0x00)
            {
                // 0x00 地址可以接收任意地址码
            }

            // 4. 验证命令码
            byte responseCmd = headerBuffer[3];
            byte expectedSuccessCmd = (byte)(originalCmd | RSP_SUCCESS_FLAG);
            byte expectedErrorCmd = (byte)(originalCmd | RSP_ERROR_FLAG);

            bool isSuccess = (responseCmd == expectedSuccessCmd);
            bool isError = (responseCmd == expectedErrorCmd);

            if (!isSuccess && !isError)
            {
                throw new CH9329Exception(Ch9329Error.CmdErrCmd, $"响应命令码不匹配。收到: 0x{responseCmd:X2}, 期望: 0x{expectedSuccessCmd:X2} 或 0x{expectedErrorCmd:X2}。");
            }

            // 5. 读取数据和校验和
            byte dataLen = headerBuffer[4];
            byte[] dataBuffer = _bufferPool.Rent(dataLen + 1);
            mem = dataBuffer.AsMemory(0, dataLen + 1);

            try
            {
                bytesRead = 0;

                if (dataLen + 1 > 0)
                {
                    while (bytesRead < dataLen + 1)
                    {
                        bytesRead += await _serialPort.BaseStream.ReadAsync(mem);
                    }
                }

                byte receivedSum = dataBuffer[dataLen];

                // 6. 验证校验和（使用优化的计算方法）
                byte calculatedSum = CalculateChecksum(headerBuffer.AsSpan(0, 5));
                calculatedSum += CalculateChecksum(dataBuffer.AsSpan(0, dataLen));

                if (receivedSum != calculatedSum)
                {
                    throw new CH9329Exception(Ch9329Error.CmdErrSum, $"响应校验和错误。收到: 0x{receivedSum:X2}, 计算: 0x{calculatedSum:X2}。");
                }

                // 7. 处理错误响应
                if (isError)
                {
                    Ch9329Error errorCode = (dataLen > 0) ? (Ch9329Error)dataBuffer[0] : Ch9329Error.CmdErrOperate;
                    throw new CH9329Exception(errorCode, $"芯片返回错误。状态码: 0x{errorCode:X2}");
                }

                // 8. 返回成功数据
                return mem[..dataLen].ToArray();
            }
            finally
            {
                _bufferPool.Return(dataBuffer);
            }
        }
        finally
        {
            _bufferPool.Return(headerBuffer);
        }
    }

    #endregion

    #region 性能优化扩展方法 (Performance Optimizations)

    /// <summary>
    /// 批量发送键盘按键序列（性能优化，减少锁竞争）。
    /// </summary>
    /// <param name="keySequence">键盘操作序列（修饰键和按键数组对）。</param>
    public async Task SendKeyboardSequenceAsync(IEnumerable<(KeyModifier modifiers, byte[] keys)> keySequence)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException("串口未打开。");
            }

            foreach (var (modifiers, keys) in keySequence)
            {
                _serialPort.DiscardInBuffer();

                byte[] data = _bufferPool.Rent(8);
                try
                {
                    data[0] = (byte)modifiers;
                    data[1] = 0x00;

                    for (int i = 0; i < 6; i++)
                    {
                        data[i + 2] = (i < keys.Length) ? keys[i] : (byte)0x00;
                    }

                    int dataLength = 8;
                    int frameSize = 6 + dataLength;
                    byte[] frameBuffer = _bufferPool.Rent(frameSize);
                    try
                    {
                        Span<byte> frame = frameBuffer.AsSpan(0, frameSize);
                        ReadOnlySpan<byte> dataSpan = data.AsSpan(0, 8);
                        BuildFrameInternal(frame, CMD_SEND_KB_GENERAL_DATA, dataSpan);

                        await _serialPort.BaseStream.WriteAsync(frameBuffer.AsMemory(0, frameSize), CancellationToken.None);

                        byte[] response = await ReadResponseAsync(CMD_SEND_KB_GENERAL_DATA);
                        ValidateSimpleResponse(response, "SendKeyboardSequenceAsync");
                    }
                    finally
                    {
                        _bufferPool.Return(frameBuffer);
                    }
                }
                finally
                {
                    _bufferPool.Return(data);
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 批量发送鼠标移动序列（性能优化）。
    /// </summary>
    /// <param name="mouseSequence">鼠标操作序列。</param>
    public async Task SendMouseSequenceAsync(IEnumerable<(MouseButton buttons, sbyte dx, sbyte dy, sbyte wheel)> mouseSequence)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException("串口未打开。");
            }

            foreach (var (buttons, dx, dy, wheel) in mouseSequence)
            {
                _serialPort.DiscardInBuffer();

                byte[] data = _bufferPool.Rent(5);
                try
                {
                    data[0] = 0x01;
                    data[1] = (byte)buttons;
                    data[2] = (byte)dx;
                    data[3] = (byte)dy;
                    data[4] = (byte)wheel;

                    int dataLength = 5;
                    int frameSize = 6 + dataLength;
                    byte[] frameBuffer = _bufferPool.Rent(frameSize);
                    try
                    {
                        Span<byte> frame = frameBuffer.AsSpan(0, frameSize);
                        ReadOnlySpan<byte> dataSpan = data.AsSpan(0, 5);
                        BuildFrameInternal(frame, CMD_SEND_MS_REL_DATA, dataSpan);

                        await _serialPort.BaseStream.WriteAsync(frameBuffer.AsMemory(0, frameSize), CancellationToken.None);

                        byte[] response = await ReadResponseAsync(CMD_SEND_MS_REL_DATA);
                        ValidateSimpleResponse(response, "SendMouseSequenceAsync");
                    }
                    finally
                    {
                        _bufferPool.Return(frameBuffer);
                    }
                }
                finally
                {
                    _bufferPool.Return(data);
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 预热缓存（预先构建常用的帧）。
    /// </summary>
    public void WarmupCache()
    {
        // 预先构建键盘释放帧
        if (_cachedKeyReleaseFrame == null)
        {
            byte[] data = new byte[8];
            _cachedKeyReleaseFrame = BuildFrame(CMD_SEND_KB_GENERAL_DATA, data);
        }

        // 预先构建鼠标释放帧
        if (_cachedMouseReleaseFrame == null)
        {
            byte[] data = new byte[5];
            data[0] = 0x01;
            _cachedMouseReleaseFrame = BuildFrame(CMD_SEND_MS_REL_DATA, data);
        }
    }

    /// <summary>
    /// 快速发送鼠标释放命令（使用缓存帧）。
    /// </summary>
    public async Task SendMouseReleaseAsync()
    {
        if (_cachedMouseReleaseFrame == null)
        {
            byte[] data = new byte[5];
            data[0] = 0x01;
            _cachedMouseReleaseFrame = BuildFrame(CMD_SEND_MS_REL_DATA, data);
        }

        byte[] response = await SendFrameDirectAsync(_cachedMouseReleaseFrame, CMD_SEND_MS_REL_DATA);
        ValidateSimpleResponse(response, "SendMouseReleaseAsync");
    }

    #endregion

    /// <summary>
    /// 释放由 CH9329Controller 占用的资源。
    /// </summary>
    public void Dispose()
    {
        Close();
        _serialPort.Dispose();
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}
