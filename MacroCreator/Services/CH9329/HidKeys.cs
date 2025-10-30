/*
 * CH9329 串口通信协议 C#/.NET 封装类
 *
 * 本类根据 "CH9329芯片串口通信协议V1.0.pdf" 文档实现。
 * 它提供了一个通过 .NET SerialPort 与 CH9329 芯片进行通信的接口。
 */

namespace MacroCreator.Services.CH9329;

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