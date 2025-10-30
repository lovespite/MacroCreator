/*
 * CH9329 输入模拟器 - 高级封装类
 * 
 * 提供简化的鼠标和键盘操作接口，基于 Ch9329Controller 实现。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// CH9329 输入模拟器，提供高级鼠标和键盘操作接口。
/// </summary>
public class InputSimulator : IDisposable
{
    private readonly Ch9329Controller _controller;

    public Ch9329Controller Controller => _controller;

    // 跟踪当前按下的键和鼠标按钮状态
    private readonly HashSet<Keys> _pressedKeys = [];
    private MouseButton _pressedMouseButtons = MouseButton.None;

    // 跟踪绝对鼠标位置（用于 MoveTo）
    private int _currentX = 0;
    private int _currentY = 0;

    // 屏幕分辨率（用于绝对坐标转换，默认 1920x1080）
    private int _screenWidth = 1920;
    private int _screenHeight = 1080;

    /// <summary>
    /// 初始化 InputSimulator 的新实例。
    /// </summary>
    /// <param name="controller">底层 CH9329 控制器实例。</param>
    public InputSimulator(Ch9329Controller controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <summary>
    /// 初始化 InputSimulator 的新实例（创建新的控制器）。
    /// </summary>
    /// <param name="portName">串口名称 (例如 "COM3")</param>
    /// <param name="baudRate">波特率 (默认为 9600)</param>
    /// <param name="chipAddress">芯片地址 (默认为 0x00)</param>
    public InputSimulator(string portName, int baudRate = 9600, byte chipAddress = 0x00)
    {
        _controller = new Ch9329Controller(portName, baudRate, chipAddress);
    }

    /// <summary>
    /// 设置屏幕分辨率（用于绝对坐标转换）。
    /// </summary>
    /// <param name="width">屏幕宽度（像素）。</param>
    /// <param name="height">屏幕高度（像素）。</param>
    public void SetScreenResolution(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("屏幕分辨率必须大于 0。");
        }
        _screenWidth = width;
        _screenHeight = height;
    }

    /// <summary>
    /// 打开串口连接。
    /// </summary>
    public void Open() => _controller.Open();

    /// <summary>
    /// 关闭串口连接。
    /// </summary>
    public void Close() => _controller.Close();

    #region 鼠标操作 (Mouse Operations)

    /// <summary>
    /// 相对移动鼠标。
    /// </summary>
    /// <param name="dx">X 轴相对移动量 (-127 到 +127)。</param>
    /// <param name="dy">Y 轴相对移动量 (-127 到 +127)。</param>
    public async Task MouseMove(int dx, int dy)
    {
        // 限制范围
        sbyte deltaX = ClampToSByte(dx);
        sbyte deltaY = ClampToSByte(dy);

        // 更新内部位置跟踪（近似）
        _currentX += deltaX;
        _currentY += deltaY;

        await _controller.SendRelativeMouseAsync(_pressedMouseButtons, deltaX, deltaY, 0);
    }

    /// <summary>
    /// 移动鼠标到绝对坐标位置。
    /// </summary>
    /// <param name="x">目标 X 坐标（屏幕像素）。</param>
    /// <param name="y">目标 Y 坐标（屏幕像素）。</param>
    public async Task MouseMoveTo(int x, int y)
    {
        // 转换屏幕坐标到 CH9329 绝对坐标 (0-4095)
        ushort absX = ConvertToAbsoluteX(x);
        ushort absY = ConvertToAbsoluteY(y);

        // 更新内部位置跟踪
        _currentX = x;
        _currentY = y;

        await _controller.SendAbsoluteMouseAsync(_pressedMouseButtons, absX, absY, 0);
    }

    /// <summary>
    /// 按下鼠标按键（不释放）。
    /// </summary>
    /// <param name="button">要按下的鼠标按键。</param>
    public async Task MouseDown(MouseButton button)
    {
        _pressedMouseButtons |= button;
        await _controller.SendRelativeMouseAsync(_pressedMouseButtons, 0, 0, 0);
    }

    /// <summary>
    /// 释放鼠标按键。
    /// </summary>
    /// <param name="button">要释放的鼠标按键。</param>
    public async Task MouseUp(MouseButton button)
    {
        _pressedMouseButtons &= ~button;
        await _controller.SendRelativeMouseAsync(_pressedMouseButtons, 0, 0, 0);
    }

    /// <summary>
    /// 点击鼠标按键（按下并释放）。
    /// </summary>
    /// <param name="button">要点击的鼠标按键。</param>
    /// <param name="delayMs">按下和释放之间的延迟（毫秒），默认 50ms。</param>
    public async Task MouseClick(MouseButton button, int delayMs = 50)
    {
        await MouseDown(button);
        if (delayMs > 0)
        {
            await Task.Delay(delayMs);
        }
        await MouseUp(button);
    }

    /// <summary>
    /// 双击鼠标按键。
    /// </summary>
    /// <param name="button">要双击的鼠标按键。</param>
    /// <param name="delayMs">两次点击之间的延迟（毫秒），默认 100ms。</param>
    public async Task MouseDoubleClick(MouseButton button, int delayMs = 100)
    {
        await MouseClick(button);
        if (delayMs > 0)
        {
            await Task.Delay(delayMs);
        }
        await MouseClick(button);
    }

    /// <summary>
    /// 鼠标滚轮滚动。
    /// </summary>
    /// <param name="amount">滚动量。正值向上，负值向下。</param>
    public async Task MouseWheel(int amount)
    {
        sbyte wheelAmount = ClampToSByte(amount);
        await _controller.SendRelativeMouseAsync(_pressedMouseButtons, 0, 0, wheelAmount);
    }

    public async Task ReleaseAllMouse()
    {
        await _controller.SendMouseReleaseAsync();
    }

    #endregion

    #region 键盘操作 (Keyboard Operations)

    /// <summary>
    /// 按下键盘按键（不释放）。
    /// </summary>
    /// <param name="key">要按下的按键。</param>
    public async Task KeyDown(Keys key)
    {
        // 提取修饰键和普通键
        var (modifiers, normalKey) = ExtractKeyComponents(key);

        // 添加到已按下的键集合
        if (normalKey != Keys.None)
        {
            _pressedKeys.Add(normalKey);
        }

        // 构建当前按键数组（最多 6 个）
        byte[] keyCodes = [.. _pressedKeys.Select(HidKeys.Map).Where(k => k != 0x00).Take(6)];
        await _controller.SendKeyboardDataAsync(modifiers, keyCodes);
    }

    /// <summary>
    /// 释放键盘按键。
    /// </summary>
    /// <param name="key">要释放的按键。</param>
    public async Task KeyUp(Keys key)
    {
        // 提取修饰键和普通键
        var (modifiers, normalKey) = ExtractKeyComponents(key);

        // 从已按下的键集合中移除
        if (normalKey != Keys.None)
        {
            _pressedKeys.Remove(normalKey);
        }

        // 如果还有其他按键按下，发送更新状态；否则发送全释放
        if (_pressedKeys.Count > 0)
        {
            byte[] keyCodes = [.. _pressedKeys.Select(HidKeys.Map).Where(k => k != 0x00).Take(6)];
            await _controller.SendKeyboardDataAsync(KeyModifier.None, keyCodes);
        }
        else
        {
            await ReleaseAllKeys();
        }
    }

    /// <summary>
    /// 按下并释放键盘按键。
    /// </summary>
    /// <param name="key">要按下的按键。</param>
    /// <param name="delayMs">按下和释放之间的延迟（毫秒），默认 50ms。</param>
    public async Task KeyPress(Keys key, int delayMs = 50)
    {
        await KeyDown(key);
        if (delayMs > 0) await Task.Delay(delayMs);
        await KeyUp(key);
    }

    /// <summary>
    /// 释放所有按键。
    /// </summary>
    public async Task ReleaseAllKeys()
    {
        _pressedKeys.Clear();
        await _controller.SendKeyboardDataAsync(KeyModifier.None, Array.Empty<byte>());
    }

    /// <summary>
    /// 输入文本（连续按键）。
    /// </summary>
    /// <param name="text">要输入的文本。</param>
    /// <param name="delayMs">每个按键之间的延迟（毫秒），默认 50ms。</param>
    public async Task TypeText(string text, int delayMs = 50)
    {
        if (string.IsNullOrEmpty(text)) return;

        foreach (char c in text)
        {
            Keys key = CharToKey(c);
            if (key != Keys.None)
            {
                await KeyPress(key, delayMs / 2);
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
        }
    }

    #endregion

    #region 组合操作 (Combination Operations)

    /// <summary>
    /// 按下组合键（例如 Ctrl+C）。
    /// </summary>
    /// <param name="keys">要按下的按键组合。</param>
    public async Task KeyCombination(params Keys[] keys)
    {
        if (keys == null || keys.Length == 0) return;

        // 依次按下所有键
        foreach (var key in keys)
        {
            await KeyDown(key);
            await Task.Delay(10); // 短暂延迟确保顺序
        }

        // 短暂保持
        await Task.Delay(50);

        // 反序释放所有键
        foreach (var key in keys.Reverse())
        {
            await KeyUp(key);
            await Task.Delay(10);
        }
    }

    #endregion

    #region 辅助方法 (Helper Methods)

    /// <summary>
    /// 将整数限制到 sbyte 范围 (-128 到 127)。
    /// </summary>
    private static sbyte ClampToSByte(int value)
    {
        if (value > 127) return 127;
        if (value < -128) return -128;
        return (sbyte)value;
    }

    /// <summary>
    /// 将屏幕 X 坐标转换为 CH9329 绝对坐标 (0-4095)。
    /// </summary>
    private ushort ConvertToAbsoluteX(int x)
    {
        if (x < 0) x = 0;
        if (x >= _screenWidth) x = _screenWidth - 1;
        return (ushort)((x * 4095) / _screenWidth);
    }

    /// <summary>
    /// 将屏幕 Y 坐标转换为 CH9329 绝对坐标 (0-4095)。
    /// </summary>
    private ushort ConvertToAbsoluteY(int y)
    {
        if (y < 0) y = 0;
        if (y >= _screenHeight) y = _screenHeight - 1;
        return (ushort)((y * 4095) / _screenHeight);
    }

    /// <summary>
    /// 从 Keys 枚举中提取修饰键和普通键。
    /// </summary>
    private static (KeyModifier modifiers, Keys normalKey) ExtractKeyComponents(Keys key)
    {
        KeyModifier modifiers = KeyModifier.None;
        Keys normalKey = key;

        // 检查并移除修饰键标志
        if (key.HasFlag(Keys.Control))
        {
            modifiers |= KeyModifier.LeftCtrl;
            normalKey &= ~Keys.Control;
        }
        if (key.HasFlag(Keys.Shift))
        {
            modifiers |= KeyModifier.LeftShift;
            normalKey &= ~Keys.Shift;
        }
        if (key.HasFlag(Keys.Alt))
        {
            modifiers |= KeyModifier.LeftAlt;
            normalKey &= ~Keys.Alt;
        }

        // 检查特定的修饰键
        if (normalKey == Keys.LControlKey || normalKey == Keys.ControlKey)
        {
            modifiers |= KeyModifier.LeftCtrl;
            normalKey = Keys.None;
        }
        else if (normalKey == Keys.RControlKey)
        {
            modifiers |= KeyModifier.RightCtrl;
            normalKey = Keys.None;
        }
        else if (normalKey == Keys.LShiftKey || normalKey == Keys.ShiftKey)
        {
            modifiers |= KeyModifier.LeftShift;
            normalKey = Keys.None;
        }
        else if (normalKey == Keys.RShiftKey)
        {
            modifiers |= KeyModifier.RightShift;
            normalKey = Keys.None;
        }
        else if (normalKey == Keys.LMenu || normalKey == Keys.Menu)
        {
            modifiers |= KeyModifier.LeftAlt;
            normalKey = Keys.None;
        }
        else if (normalKey == Keys.RMenu)
        {
            modifiers |= KeyModifier.RightAlt;
            normalKey = Keys.None;
        }
        else if (normalKey == Keys.LWin)
        {
            modifiers |= KeyModifier.LeftWindows;
            normalKey = Keys.None;
        }
        else if (normalKey == Keys.RWin)
        {
            modifiers |= KeyModifier.RightWindows;
            normalKey = Keys.None;
        }

        return (modifiers, normalKey);
    }

    /// <summary>
    /// 将字符转换为对应的 Keys 枚举。
    /// </summary>
    private static Keys CharToKey(char c)
    {
        return c switch
        {
            >= 'a' and <= 'z' => Keys.A + (c - 'a'),
            >= 'A' and <= 'Z' => (Keys.Shift | (Keys.A + (c - 'A'))),
            >= '0' and <= '9' => Keys.D0 + (c - '0'),
            ' ' => Keys.Space,
            '\t' => Keys.Tab,
            '\r' or '\n' => Keys.Enter,
            '.' => Keys.OemPeriod,
            ',' => Keys.Oemcomma,
            ';' => Keys.OemSemicolon,
            '\'' => Keys.OemQuotes,
            '/' => Keys.OemQuestion,
            '\\' => Keys.OemBackslash,
            '[' => Keys.OemOpenBrackets,
            ']' => Keys.OemCloseBrackets,
            '-' => Keys.OemMinus,
            '=' => Keys.Oemplus,
            '`' => Keys.Oemtilde,

            // 需要 Shift 的符号
            '!' => Keys.Shift | Keys.D1,
            '@' => Keys.Shift | Keys.D2,
            '#' => Keys.Shift | Keys.D3,
            '$' => Keys.Shift | Keys.D4,
            '%' => Keys.Shift | Keys.D5,
            '^' => Keys.Shift | Keys.D6,
            '&' => Keys.Shift | Keys.D7,
            '*' => Keys.Shift | Keys.D8,
            '(' => Keys.Shift | Keys.D9,
            ')' => Keys.Shift | Keys.D0,
            '_' => Keys.Shift | Keys.OemMinus,
            '+' => Keys.Shift | Keys.Oemplus,
            '{' => Keys.Shift | Keys.OemOpenBrackets,
            '}' => Keys.Shift | Keys.OemCloseBrackets,
            '|' => Keys.Shift | Keys.OemBackslash,
            ':' => Keys.Shift | Keys.OemSemicolon,
            '"' => Keys.Shift | Keys.OemQuotes,
            '<' => Keys.Shift | Keys.Oemcomma,
            '>' => Keys.Shift | Keys.OemPeriod,
            '?' => Keys.Shift | Keys.OemQuestion,
            '~' => Keys.Shift | Keys.Oemtilde,

            _ => Keys.None
        };
    }

    #endregion

    /// <summary>
    /// 释放资源。
    /// </summary>
    public void Dispose()
    {
        _controller?.Dispose();
        GC.SuppressFinalize(this);
    }
}
