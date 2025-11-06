using MacroCreator.Models;

namespace MacroCreator.Services;

// 常用鼠标按键和键盘修饰键常量
partial class SimulatorBase
{
    public readonly KeyConstants Kbd = new();
    public readonly MouseButtonConstants Mouse = new();

    public class MouseButtonConstants
    {
        public readonly MouseButton L = MouseButton.Left;
        public readonly MouseButton R = MouseButton.Right;
        public readonly MouseButton M = MouseButton.Middle;
    }

    public class KeyConstants
    {
        public readonly Keys Tab = Keys.Tab;
        public readonly Keys Back = Keys.Back;
        public readonly Keys CapsLock = Keys.CapsLock;
        public readonly Keys Enter = Keys.Enter;
        public readonly Keys Space = Keys.Space;
        public readonly Keys Escape = Keys.Escape;

        public readonly Keys Up = Keys.Up;
        public readonly Keys Down = Keys.Down;
        public readonly Keys Left = Keys.Left;
        public readonly Keys Right = Keys.Right;

        public readonly Keys Insert = Keys.Insert;
        public readonly Keys Delete = Keys.Delete;
        public readonly Keys Home = Keys.Home;
        public readonly Keys End = Keys.End;
        public readonly Keys PageUp = Keys.PageUp;
        public readonly Keys PageDown = Keys.PageDown;

        #region Modifier Keys

        public readonly KeyModifier LShift = KeyModifier.LeftShift;
        public readonly KeyModifier RShift = KeyModifier.RightShift;
        public readonly KeyModifier LCtrl = KeyModifier.LeftCtrl;
        public readonly KeyModifier RCtrl = KeyModifier.RightCtrl;
        public readonly KeyModifier LAlt = KeyModifier.LeftAlt;
        public readonly KeyModifier RAlt = KeyModifier.RightAlt;
        public readonly KeyModifier LWin = KeyModifier.LeftWindows;
        public readonly KeyModifier RWin = KeyModifier.RightWindows;

        #endregion

        #region OemKeys

        public readonly Keys Oemcomma = Keys.Oemcomma; // ,<
        public readonly Keys OemPeriod = Keys.OemPeriod; // .>
        public readonly Keys OemQuestion = Keys.OemQuestion; // /?

        public readonly Keys OemSemicolon = Keys.OemSemicolon; // ;:
        public readonly Keys OemQuotes = Keys.OemQuotes; // '"

        public readonly Keys OemOpenBrackets = Keys.OemOpenBrackets; // [{
        public readonly Keys OemCloseBrackets = Keys.OemCloseBrackets; // ]}
        public readonly Keys OemPipe = Keys.OemPipe; // \|

        public readonly Keys Oemtilde = Keys.Oemtilde; // `~
        public readonly Keys OemMinus = Keys.OemMinus; // -_
        public readonly Keys Oemplus = Keys.Oemplus; // =+

        #endregion

        #region Numbers

        public readonly Keys D0 = Keys.D0;
        public readonly Keys D1 = Keys.D1;
        public readonly Keys D2 = Keys.D2;
        public readonly Keys D3 = Keys.D3;
        public readonly Keys D4 = Keys.D4;
        public readonly Keys D5 = Keys.D5;
        public readonly Keys D6 = Keys.D6;
        public readonly Keys D7 = Keys.D7;
        public readonly Keys D8 = Keys.D8;
        public readonly Keys D9 = Keys.D9;

        #endregion

        #region Function Keys

        public readonly Keys F1 = Keys.F1;
        public readonly Keys F2 = Keys.F2;
        public readonly Keys F3 = Keys.F3;
        public readonly Keys F4 = Keys.F4;
        public readonly Keys F5 = Keys.F5;
        public readonly Keys F6 = Keys.F6;
        public readonly Keys F7 = Keys.F7;
        public readonly Keys F8 = Keys.F8;
        public readonly Keys F9 = Keys.F9;
        public readonly Keys F10 = Keys.F10;
        public readonly Keys F11 = Keys.F11;
        public readonly Keys F12 = Keys.F12;
        public readonly Keys F13 = Keys.F13;
        public readonly Keys F14 = Keys.F14;
        public readonly Keys F15 = Keys.F15;
        public readonly Keys F16 = Keys.F16;
        public readonly Keys F17 = Keys.F17;
        public readonly Keys F18 = Keys.F18;
        public readonly Keys F19 = Keys.F19;
        public readonly Keys F20 = Keys.F20;
        public readonly Keys F21 = Keys.F21;
        public readonly Keys F22 = Keys.F22;
        public readonly Keys F23 = Keys.F23;
        public readonly Keys F24 = Keys.F24;

        #endregion

        #region Alphanumeric Keys

        public readonly Keys A = Keys.A;
        public readonly Keys B = Keys.B;
        public readonly Keys C = Keys.C;
        public readonly Keys D = Keys.D;
        public readonly Keys E = Keys.E;
        public readonly Keys F = Keys.F;
        public readonly Keys G = Keys.G;
        public readonly Keys H = Keys.H;
        public readonly Keys I = Keys.I;
        public readonly Keys J = Keys.J;
        public readonly Keys K = Keys.K;
        public readonly Keys L = Keys.L;
        public readonly Keys M = Keys.M;
        public readonly Keys N = Keys.N;
        public readonly Keys O = Keys.O;
        public readonly Keys P = Keys.P;
        public readonly Keys Q = Keys.Q;
        public readonly Keys R = Keys.R;
        public readonly Keys S = Keys.S;
        public readonly Keys T = Keys.T;
        public readonly Keys U = Keys.U;
        public readonly Keys V = Keys.V;
        public readonly Keys W = Keys.W;
        public readonly Keys X = Keys.X;
        public readonly Keys Y = Keys.Y;
        public readonly Keys Z = Keys.Z;

        #endregion

        #region Numpad Keys

        public readonly Keys NumPad0 = Keys.NumPad0;
        public readonly Keys NumPad1 = Keys.NumPad1;
        public readonly Keys NumPad2 = Keys.NumPad2;
        public readonly Keys NumPad3 = Keys.NumPad3;
        public readonly Keys NumPad4 = Keys.NumPad4;
        public readonly Keys NumPad5 = Keys.NumPad5;
        public readonly Keys NumPad6 = Keys.NumPad6;
        public readonly Keys NumPad7 = Keys.NumPad7;
        public readonly Keys NumPad8 = Keys.NumPad8;
        public readonly Keys NumPad9 = Keys.NumPad9;

        #endregion
    }
}