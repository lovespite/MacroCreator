using MacroCreator.Controller;
using MacroCreator.Models;
using MacroCreator.Models.Events;
using MacroCreator.Native;
using MacroCreator.Services;
using MacroCreator.Services.CH9329;
using MacroScript.Dsl;
using MacroScript.Utils;

namespace MacroScript.Interactive;


partial class InteractiveInterface
{
    public static ISimulator ConnectSimulator(string? comPortName)
    {
        if (comPortName is not null)
        {
            var simulator = Ch9329Simulator.Open(comPortName);
            simulator.Controller.Open();
            simulator.Controller.WarmupCache();
            return simulator;
        }

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return new Win32LocalMachineSimulator();
        }
        else
        {
            ConsoleHelper.Instance.PrintWarning("当前操作系统不支持本地模拟器");
            return NopSimulator.Instance;
        }
    }

    public static async Task<MacroController> GetMacroController(string? comPort)
    {
        var simulator = ConnectSimulator(comPort);

        if (simulator is Ch9329Simulator ch9329)
        {
            ConsoleHelper.Instance.PrintLine($"正在连接设备 {comPort} ...");
            var info = await ch9329.Controller.GetInfoAsync();

            if (info.UsbStatus == UsbStatus.NotConnected)
            {
                throw new InvalidOperationException("HID设备未连接到目标主机，或未被正确识别，请检查连接后重试");
            }
            ConsoleHelper.Instance.PrintInfo($"已重定向到设备 {comPort}, 状态:\n{info}");
        }

        var controller = new MacroController(new SystemTimer(), simulator);

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            controller.PlaybackService.SetInterpreterVariable("clipboard", new Win32.Win32Clipboard());

        controller.PlaybackService.SetInterpreterVariable("hid", simulator);
        controller.OnPrint += ConsoleHelper.Instance.Print;
        controller.OnPrintLine += ConsoleHelper.Instance.PrintLine;

        return controller;
    }

    private static async Task<List<MacroEvent>> CompileAsync(string inputFile)
    {
        var tcs = new TaskCompletionSource<List<MacroEvent>>();

        var t = new Thread(() =>
        {
            try
            {
                var collection = Scripting.Compile(inputFile);

                if (collection.Count <= 0)
                    tcs.SetException(new InvalidDataException("事件序列为空"));
                else
                    tcs.SetResult(collection);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        t.IsBackground = true;
        t.Name = "CompileThread";
        t.Start();

        return await tcs.Task;
    }

    internal static void InstallCustomParameterDeserializers()
    {
        var customModifierKeyMap = new Dictionary<string, Keys> {
            { "#", Keys.LWin },
            { "$", Keys.LControlKey },
            { "%", Keys.LMenu },
            { "^", Keys.LShiftKey },
        };

        ParameterDeserializer.SetCustomDeserializer(typeof(MouseButton), s =>
        {
            if (Enum.TryParse<MouseButton>(s, true, out var btn)) return btn;
            s = s.ToLowerInvariant();
            if (string.Equals(s, "all", StringComparison.OrdinalIgnoreCase))
            {
                return MouseButton.Left | MouseButton.Right | MouseButton.Middle;
            }

            if (int.TryParse(s, out var intVal))
            {
                return (MouseButton)intVal;
            }

            MouseButton flag = MouseButton.None;
            if (s.Contains('l'))
                flag |= MouseButton.Left;
            if (s.Contains('r'))
                flag |= MouseButton.Right;
            if (s.Contains('m'))
                flag |= MouseButton.Middle;

            return flag;

        });

        Keys ParseKeysEnum(string s)
        {
            if (customModifierKeyMap.TryGetValue(s, out var key))
            {
                return key;
            }
            return Enum.Parse<Keys>(s, true);
        }

        KeyModifier ParseKeyModifierEnum(string s)
        {
            var mod = KeyModifier.None;

            foreach (var c in s)
            {
                mod |= c switch
                {
                    '#' => KeyModifier.LeftWindows,
                    '$' => KeyModifier.LeftCtrl,
                    '%' => KeyModifier.LeftAlt,
                    '^' => KeyModifier.LeftShift,
                    _ => KeyModifier.None
                };
            }

            return mod;
        }

        ParameterDeserializer.SetCustomDeserializer(typeof(Keys[]), s =>
        {
            return s.Split([',', '+'], StringSplitOptions.RemoveEmptyEntries).Select(ParseKeysEnum).ToArray();
        });

        ParameterDeserializer.SetCustomDeserializer(typeof(Keys), s => ParseKeysEnum(s));


        ParameterDeserializer.SetCustomDeserializer(typeof(KeyModifier), s => ParseKeyModifierEnum(s));
    }
}