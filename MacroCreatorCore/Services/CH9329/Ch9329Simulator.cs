/*
 * CH9329 输入模拟器 - 高级封装类
 * 
 * 提供简化的鼠标和键盘操作接口，基于 Ch9329Controller 实现。
 */
using System.Diagnostics;
using MacroCreator.Models;

namespace MacroCreator.Services.CH9329;

/// <summary>
/// CH9329 输入模拟器，提供高级鼠标和键盘操作接口。
/// </summary>
public class Ch9329Simulator : SimulatorBase
{
    private readonly Ch9329Controller _ch9329Controller;
    public override string Name => $"CH9329_{_ch9329Controller.PortName}";
    public Ch9329Controller Controller => _ch9329Controller;

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
    public Ch9329Simulator(Ch9329Controller controller)
    {
        _ch9329Controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <summary>
    /// 使用指定的串口参数打开一个新的 InputSimulator 实例。 
    /// </summary>
    /// <param name="comPort"></param>
    /// <param name="baudRate"></param>
    /// <param name="chipAddress"></param>
    /// <returns></returns>
    public static Ch9329Simulator Open(string comPort, int baudRate = 9600, byte chipAddress = 0x00)
    {
        var ch9329Controller = new Ch9329Controller(comPort, baudRate, chipAddress);

        try
        {
            ch9329Controller.Open();
        }
        catch (Exception ex)
        {
            ch9329Controller.Dispose();

#if DEBUG
            Debug.WriteLine($"Failed to open CH9329 controller on {comPort}: {ex}");
#endif

            throw;
        }

        var simulator = new Ch9329Simulator(ch9329Controller);
        ch9329Controller.WarmupCache();

        return simulator;
    }

    public override void SetScreenResolution(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("屏幕分辨率必须大于 0");
        }
        _screenWidth = width;
        _screenHeight = height;
    }

    /// <summary>
    /// 打开串口连接。
    /// </summary>
    public void Open() => _ch9329Controller.Open();

    /// <summary>
    /// 关闭串口连接。
    /// </summary>
    public void Close() => _ch9329Controller.Close();

    #region 鼠标操作 (Mouse Operations)

    public override async Task MouseMove(int dx, int dy)
    {
        // 限制范围
        sbyte deltaX = ClampToSByte(dx);
        sbyte deltaY = ClampToSByte(dy);

        // 更新内部位置跟踪（近似）
        _currentX += deltaX;
        _currentY += deltaY;

        await _ch9329Controller.SendRelativeMouseAsync(_pressedMouseButtons, deltaX, deltaY, 0);
    }

    public override async Task MouseMoveTo(int x, int y)
    {
        // 转换屏幕坐标到 CH9329 绝对坐标 (0-4095)
        ushort absX = ConvertToAbsoluteX(x);
        ushort absY = ConvertToAbsoluteY(y);

        // 更新内部位置跟踪
        _currentX = x;
        _currentY = y;

        await _ch9329Controller.SendAbsoluteMouseAsync(_pressedMouseButtons, absX, absY, 0);
    }

    public override async Task MouseDown(MouseButton button)
    {
        _pressedMouseButtons |= button;
        await _ch9329Controller.SendRelativeMouseAsync(_pressedMouseButtons, 0, 0, 0);
    }

    public override async Task MouseUp(MouseButton button)
    {
        _pressedMouseButtons &= ~button;
        await _ch9329Controller.SendRelativeMouseAsync(_pressedMouseButtons, 0, 0, 0);
    }

    public override async Task MouseWheel(int amount)
    {
        sbyte wheelAmount = ClampToSByte(amount);
        await _ch9329Controller.SendRelativeMouseAsync(_pressedMouseButtons, 0, 0, wheelAmount);
    }

    public override async Task ReleaseAllMouse()
    {
        await _ch9329Controller.SendMouseReleaseAsync();
    }

    #endregion

    #region 键盘操作 (Keyboard Operations)

    public override async Task KeyDown(params Keys[] key)
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
        await _ch9329Controller.SendKeyboardDataAsync(modifiers, keyCodes);
    }

    public override async Task KeyUp(params Keys[] key)
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
            await _ch9329Controller.SendKeyboardDataAsync(KeyModifier.None, keyCodes);
        }
        else
        {
            await ReleaseAllKeys();
        }
    }

    [Obsolete("Use KeyCombination(KeyModifier modifier, Keys key) instead.")]
    public override async Task KeyCombination(params Keys[] keys)
    {
        await KeyDown(keys);
        await Task.Delay(50);
        await ReleaseAllKeys();
    }

    public override async Task KeyCombination(KeyModifier modifier, Keys key)
    {
        await _ch9329Controller.SendKeyboardDataAsync(modifier, [HidKeys.Map(key)]);
        await Task.Delay(50);
        await ReleaseAllKeys();
    }

    public override async Task ReleaseAllKeys()
    {
        _pressedKeys.Clear();
        await _ch9329Controller.SendKeyboardDataAsync(KeyModifier.None, Array.Empty<byte>());
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
    private static (KeyModifier modifiers, Keys normalKey) ExtractKeyComponents(Keys[] keys)
    {
        KeyModifier modifiers = KeyModifier.None;
        Keys normalKey = Keys.None;

        foreach (var k in keys)
        {
            var mod = MapModifierKey(k);
            if (mod != KeyModifier.None)
            {
                modifiers |= mod;
            }
            else
            {
                normalKey = k;
            }
        }

        return (modifiers, normalKey);
    }

    private static KeyModifier MapModifierKey(Keys k)
    {
        return k switch
        {
            Keys.LControlKey => KeyModifier.LeftCtrl,
            Keys.RControlKey => KeyModifier.RightCtrl,
            Keys.LShiftKey => KeyModifier.LeftShift,
            Keys.RShiftKey => KeyModifier.RightShift,
            Keys.LWin => KeyModifier.LeftWindows,
            Keys.RWin => KeyModifier.RightWindows,
            Keys.LMenu => KeyModifier.LeftAlt,
            Keys.RMenu => KeyModifier.RightAlt,
            _ => KeyModifier.None
        };
    }

    #endregion

    /// <summary>
    /// 释放资源。
    /// </summary>
    public override void Dispose()
    {
        _ch9329Controller?.Dispose();
        base.Dispose();
    }
}
