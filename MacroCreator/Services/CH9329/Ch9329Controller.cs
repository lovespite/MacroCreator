/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

using System.IO.Ports;
using System.Text;

namespace MacroCreator.Services.CH9329;

/// <summary>
/// CH9329 芯片串口通信控制器。
/// </summary>
public class Ch9329Controller : IDisposable
{
    private readonly byte _chipAddress;
    private readonly SerialPort _serialPort;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private const byte FRAME_HEAD_1 = 0x57;
    private const byte FRAME_HEAD_2 = 0xAB;
    private const int DEFAULT_TIMEOUT = 500; // 默认超时时间 (ms)

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
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }
    }

    #region 公共命令 (Public Commands)

    /// <summary>
    /// (CMD_GET_INFO) 异步获取芯片版本号、USB枚举状态和键盘指示灯状态。
    /// </summary>
    /// <returns>包含芯片信息的 <see cref="ChipInfo"/> 对象。</returns>
    public async Task<ChipInfo> GetInfoAsync()
    {
        byte[] response = await SendCommandAsync(CMD_GET_INFO, null);

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
        if (keys == null) keys = new byte[6];
        if (keys.Length > 6) throw new ArgumentException("最多只能指定 6 个普通按键。", nameof(keys));

        byte[] data = new byte[8];
        data[0] = (byte)modifiers;
        data[1] = 0x00; // 保留字节

        // 填充按键
        for (int i = 0; i < 6; i++)
        {
            data[i + 2] = (i < keys.Length) ? keys[i] : (byte)0x00;
        }

        byte[] response = await SendCommandAsync(CMD_SEND_KB_GENERAL_DATA, data);
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
        if (keys == null) keys = new Keys[0];
        if (keys.Length > 6) throw new ArgumentException("最多只能指定 6 个普通按键。", nameof(keys));

        List<byte> hidKeysList = new List<byte>(6);
        foreach (Keys key in keys)
        {
            var hidCode = HidKeys.Map(key);
            if (hidCode == 0x00) break;
            if (!hidKeysList.Contains(hidCode))
            {
                hidKeysList.Add(hidCode);
            }
            else
            {
                throw new ArgumentException($"按键 '{key}' 重复出现。每个按键只能指定一次。", nameof(keys));
            }
        }

        // 
        if (hidKeysList.Count > 6)
        {
            // 这不应该发生，因为我们有 keys.Length > 6 的检查，但作为安全措施
            throw new ArgumentException("按键转换后超过 6 个唯一 HID 按键。", nameof(keys));
        }

        // 调用原始方法，它会处理填充到 6 个字节
        await SendKeyboardDataAsync(modifiers, hidKeysList.ToArray());
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
        ValidateSimpleResponse(response, "SendMediaKeyAsync");
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

        byte[] data =
        [
            0x02, // 固定值
            (byte)buttons,
            (byte)(x & 0xFF),         // X 坐标低字节
            (byte)((x >> 8) & 0xFF), // X 坐标高字节
            (byte)(y & 0xFF),         // Y 坐标低字节
            (byte)((y >> 8) & 0xFF), // Y 坐标高字节
            (byte)wheel,
        ];
        byte[] response = await SendCommandAsync(CMD_SEND_MS_ABS_DATA, data);
        ValidateSimpleResponse(response, "SendAbsoluteMouseAsync");
    }

    /// <summary>
    /// (CMD_SEND_MS_REL_DATA) 异步发送相对鼠标数据（移动、点击、滚轮）。
    /// </summary>
    /// <param name="buttons">鼠标按键 (例如 <see cref="MouseButton"/>)。</param>
    /// <param name="x">X轴相对移动 (-127 到 +127)。</param>
    /// <param name="y">Y轴相对移动 (-127 到 +127)。</param>
    /// <param name="wheel">滚轮滚动。正值向上，负值向下。</param>
    public async Task SendRelativeMouseAsync(MouseButton buttons, sbyte x, sbyte y, sbyte wheel)
    {
        byte[] data =
        [
            0x01, // 固定值
            (byte)buttons,
            (byte)x,
            (byte)y,
            (byte)wheel,
        ];
        byte[] response = await SendCommandAsync(CMD_SEND_MS_REL_DATA, data);
        ValidateSimpleResponse(response, "SendRelativeMouseAsync");
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
        ValidateSimpleResponse(response, "SendCustomHidDataAsync");
    }

    /// <summary>
    /// (CMD_GET_PARA_CFG) 异步获取芯片当前参数配置信息。
    /// </summary>
    /// <returns>包含 50 字节原始配置数据的字节数组。</returns>
    public async Task<byte[]> GetConfigAsync()
    {
        byte[] response = await SendCommandAsync(CMD_GET_PARA_CFG, null);
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
        ValidateSimpleResponse(response, "SetConfigAsync");
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
        ValidateSimpleResponse(response, "SetUsbStringAsync");
    }

    /// <summary>
    /// (CMD_SET_DEFAULT_CFG) 异步恢复出厂默认配置。
    /// </summary>
    public async Task RestoreDefaultsAsync()
    {
        byte[] response = await SendCommandAsync(CMD_SET_DEFAULT_CFG, null);
        ValidateSimpleResponse(response, "RestoreDefaultsAsync");
    }

    /// <summary>
    /// (CMD_RESET) 异步复位芯片。
    /// </summary>
    public async Task ResetAsync()
    {
        byte[] response = await SendCommandAsync(CMD_RESET, null);
        ValidateSimpleResponse(response, "ResetAsync");
    }

    #endregion

    #region 私有辅助方法 (Private Helpers)

    /// <summary>
    /// 验证简单的成功/失败响应 (数据长度为 1，内容为 0x00)。
    /// </summary>
    private static void ValidateSimpleResponse(byte[] response, string commandName)
    {
        if (response.Length != 1 || response[0] != (byte)Ch9329Error.Success)
        {
            throw new CH9329Exception(
                (Ch9329Error)(response.Length > 0 ? response[0] : 0xFF),
                $"{commandName} 执行失败或响应无效。状态码: 0x{(response.Length > 0 ? response[0] : 0xFF):X2}"
            );
        }
    }

    /// <summary>
    /// 构造一个完整的串口命令帧。
    /// </summary>
    private byte[] BuildFrame(byte cmd, byte[]? data)
    {
        int dataLength = (data?.Length ?? 0);
        if (dataLength > 64) throw new ArgumentException("数据长度不能超过 64 字节。");

        // 帧头(2) + 地址(1) + 命令(1) + 长度(1) + 数据(N) + 校验(1)
        byte[] frame = new byte[6 + dataLength];

        frame[0] = FRAME_HEAD_1;
        frame[1] = FRAME_HEAD_2;
        frame[2] = _chipAddress;
        frame[3] = cmd;
        frame[4] = (byte)dataLength;

        if (data is not null && dataLength > 0)
        {
            Buffer.BlockCopy(data, 0, frame, 5, dataLength);
        }

        // 计算校验和 (从 HEAD 到 DATA)
        byte checksum = 0;
        for (int i = 0; i < frame.Length - 1; i++)
        {
            checksum += frame[i];
        }
        frame[^1] = checksum;

        return frame;
    }

    /// <summary>
    /// 异步发送命令并等待响应。
    /// </summary>
    private async Task<byte[]> SendCommandAsync(byte cmd, byte[]? data)
    {
        await _lock.WaitAsync(); // 确保同一时间只有一个命令在执行
        try
        {
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException("串口未打开。");
            }

            _serialPort.DiscardInBuffer(); // 清空接收缓冲区
            byte[] frame = BuildFrame(cmd, data);

            // 发送命令
            await _serialPort.BaseStream.WriteAsync(frame, 0, frame.Length, CancellationToken.None);

            // 异步读取响应
            return await ReadResponseAsync(cmd);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 异步读取和解析响应帧。
    /// </summary>
    private async Task<byte[]> ReadResponseAsync(byte originalCmd)
    {
        byte[] header = new byte[5]; // HEAD(2) + ADDR(1) + CMD(1) + LEN(1)

        // 使用 CancellationToken 实现超时
        var cts = new CancellationTokenSource(DEFAULT_TIMEOUT);
        int bytesRead = 0;
        try
        {
            // 1. 读取帧头
            while (bytesRead < header.Length)
            {
                int read = await _serialPort.BaseStream.ReadAsync(header, bytesRead, header.Length - bytesRead, cts.Token);
                if (read == 0) throw new TimeoutException("读取响应超时。");
                bytesRead += read;
            }
        }
        catch (OperationCanceledException)
        {
            throw new CH9329Exception(Ch9329Error.CmdErrTimeout, "读取响应头超时。");
        }
        catch (Exception ex)
        {
            throw new CH9329Exception(Ch9329Error.ProtocolError, $"串口读取错误: {ex.Message}");
        }

        // 2. 验证帧头
        if (header[0] != FRAME_HEAD_1 || header[1] != FRAME_HEAD_2)
        {
            throw new CH9329Exception(Ch9329Error.CmdErrHead, "无效的响应帧头。");
        }

        // 3. 验证地址 (0xFF 是广播地址，芯片不应答，但我们检查收到的地址是否匹配)
        if (header[2] != _chipAddress && _chipAddress != 0x00)
        {
            // 0x00 地址可以接收任意地址码
        }

        // 4. 验证命令码
        byte responseCmd = header[3];
        byte expectedSuccessCmd = (byte)(originalCmd | RSP_SUCCESS_FLAG);
        byte expectedErrorCmd = (byte)(originalCmd | RSP_ERROR_FLAG);

        bool isSuccess = (responseCmd == expectedSuccessCmd);
        bool isError = (responseCmd == expectedErrorCmd);

        if (!isSuccess && !isError)
        {
            throw new CH9329Exception(Ch9329Error.CmdErrCmd, $"响应命令码不匹配。收到: 0x{responseCmd:X2}, 期望: 0x{expectedSuccessCmd:X2} 或 0x{expectedErrorCmd:X2}。");
        }

        // 5. 读取数据和校验和
        byte dataLen = header[4];
        byte[] dataAndSum = new byte[dataLen + 1];
        bytesRead = 0;

        try
        {
            if (dataAndSum.Length > 0)
            {
                while (bytesRead < dataAndSum.Length)
                {
                    int read = await _serialPort.BaseStream.ReadAsync(dataAndSum, bytesRead, dataAndSum.Length - bytesRead, cts.Token);
                    if (read == 0) throw new TimeoutException("读取响应数据超时。");
                    bytesRead += read;
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw new CH9329Exception(Ch9329Error.CmdErrTimeout, "读取响应数据超时。");
        }
        catch (Exception ex)
        {
            throw new CH9329Exception(Ch9329Error.ProtocolError, $"串口读取错误: {ex.Message}");
        }
        finally
        {
            cts.Dispose();
        }


        byte receivedSum = dataAndSum[dataLen];
        byte[] data = new byte[dataLen];
        if (dataLen > 0)
        {
            Buffer.BlockCopy(dataAndSum, 0, data, 0, dataLen);
        }

        // 6. 验证校验和
        byte calculatedSum = 0;
        foreach (byte b in header) calculatedSum += b;
        foreach (byte b in data) calculatedSum += b;

        if (receivedSum != calculatedSum)
        {
            throw new CH9329Exception(Ch9329Error.CmdErrSum, $"响应校验和错误。收到: 0x{receivedSum:X2}, 计算: 0x{calculatedSum:X2}。");
        }

        // 7. 处理错误响应
        if (isError)
        {
            Ch9329Error errorCode = (dataLen > 0) ? (Ch9329Error)data[0] : Ch9329Error.CmdErrOperate;
            throw new CH9329Exception(errorCode, $"芯片返回错误。状态码: 0x{errorCode:X2}");
        }

        // 8. 返回成功数据
        return data;
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


internal static class HidKeys
{
    private static readonly Dictionary<Keys, byte> _keyMap = new()
    {
        // 字母
        { Keys.A, 0x04 },
        { Keys.B, 0x05 },
        { Keys.C, 0x06 },
        { Keys.D, 0x07 },
        { Keys.E, 0x08 },
        { Keys.F, 0x09 },
        { Keys.G, 0x0A },
        { Keys.H, 0x0B },
        { Keys.I, 0x0C },
        { Keys.J, 0x0D },
        { Keys.K, 0x0E },
        { Keys.L, 0x0F },
        { Keys.M, 0x10 },
        { Keys.N, 0x11 },
        { Keys.O, 0x12 },
        { Keys.P, 0x13 },
        { Keys.Q, 0x14 },
        { Keys.R, 0x15 },
        { Keys.S, 0x16 },
        { Keys.T, 0x17 },
        { Keys.U, 0x18 },
        { Keys.V, 0x19 },
        { Keys.W, 0x1A },
        { Keys.X, 0x1B },
        { Keys.Y, 0x1C },
        { Keys.Z, 0x1D },

        // 数字 (主键盘)
        { Keys.D1, 0x1E },
        { Keys.D2, 0x1F },
        { Keys.D3, 0x20 },
        { Keys.D4, 0x21 },
        { Keys.D5, 0x22 },
        { Keys.D6, 0x23 },
        { Keys.D7, 0x24 },
        { Keys.D8, 0x25 },
        { Keys.D9, 0x26 },
        { Keys.D0, 0x27 },

        // 功能键
        { Keys.F1, 0x3A },
        { Keys.F2, 0x3B },
        { Keys.F3, 0x3C },
        { Keys.F4, 0x3D },
        { Keys.F5, 0x3E },
        { Keys.F6, 0x3F },
        { Keys.F7, 0x40 },
        { Keys.F8, 0x41 },
        { Keys.F9, 0x42 },
        { Keys.F10, 0x43 },
        { Keys.F11, 0x44 },
        { Keys.F12, 0x45 },

        // 控制键
        { Keys.Enter, 0x28 }, { Keys.Return, 0x28 },
        { Keys.Escape, 0x29 },
        { Keys.Back, 0x2A }, // Backspace
        { Keys.Tab, 0x2B },
        { Keys.Space, 0x2C }, // 标准 HID 码, 附录中缺失
        { Keys.OemMinus, 0x2D }, // - _
        { Keys.Oemplus, 0x2E }, // = +
        { Keys.OemOpenBrackets, 0x2F }, // [ {
        { Keys.OemCloseBrackets, 0x30 }, // ] }
        { Keys.OemBackslash, 0x31 }, // \ | (协议附录为 Keycode29, 0x31 是标准码)
        { Keys.OemSemicolon, 0x33 }, // ; :
        { Keys.OemQuotes, 0x34 }, // ' "
        { Keys.Oemtilde, 0x35 }, // ` ~
        { Keys.Oemcomma, 0x36 }, // , <
        { Keys.OemPeriod, 0x37 }, // . >
        { Keys.OemQuestion, 0x38 }, // / ?
        { Keys.CapsLock, 0x39 }, { Keys.Capital, 0x39 },

        // 导航键
        { Keys.PrintScreen, 0x46 },
        { Keys.Scroll, 0x47 },
        { Keys.Pause, 0x48 },
        { Keys.Insert, 0x49 },
        { Keys.Home, 0x4A },
        { Keys.PageUp, 0x4B }, { Keys.Prior, 0x4B },
        { Keys.Delete, 0x4C },
        { Keys.End, 0x4D },
        { Keys.PageDown, 0x4E }, { Keys.Next, 0x4E },
        { Keys.Right, 0x4F },
        { Keys.Left, 0x50 },
        { Keys.Down, 0x51 },
        { Keys.Up, 0x52 },
        { Keys.NumLock, 0x53 },

        // 小键盘
        { Keys.NumPad1, 0x59 },
        { Keys.NumPad2, 0x5A },
        { Keys.NumPad3, 0x5B },
        { Keys.NumPad4, 0x5C },
        { Keys.NumPad5, 0x5D },
        { Keys.NumPad6, 0x5E },
        { Keys.NumPad7, 0x5F },
        { Keys.NumPad8, 0x60 },
        { Keys.NumPad9, 0x61 },
        { Keys.NumPad0, 0x62 },
        { Keys.Divide, 0x54 },
        { Keys.Multiply, 0x55 },
        { Keys.Subtract, 0x56 },
        { Keys.Add, 0x57 },
        { Keys.Decimal, 0x63 },
        // Numpad Enter 协议码为 0x58, Keys.Enter 码为 0x28, 
        // 多数情况下通用，此处暂不单独映射 Numpad Enter

        // 其他
        { Keys.Apps, 0x65 }, // 菜单键
    };

    public static byte Map(Keys key)
    {
        if (_keyMap.TryGetValue(key, out byte hidCode))
        {
            return hidCode;
        }
        return 0x00; // 未映射的键返回 0x00
    }
}