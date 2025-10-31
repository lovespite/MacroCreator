﻿using System.Reflection;
using System.Text;

namespace MacroScript.Interactive;
// interpreter
public class InteractiveInterpreter : IDisposable
{
    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _buffer.Clear();
        _functions.Clear();
        GC.SuppressFinalize(this);
    }

    private ConsoleHelper Console => ConsoleHelper.Instance;

    private readonly Dictionary<string, (object, MethodInfo)> _functions = new(StringComparer.OrdinalIgnoreCase);
    private readonly StringBuilder _buffer = new();
    private bool HasBufferedText => _buffer.Length > 0;
    private volatile bool _disposed = false;

    public InteractiveInterpreter() => RegisterFunction(this);

    public void Start()
    {
        Console.WriteLine("MacroScript 交互模式");
        Console.PrintLow("-- 输入 'help' 获取帮助信息 -- ");
        Console.PrintLow("-- 输入 'exit' 退出交互模式 -- ");

        string? commandText;
        InteractiveCommand cmd;

        while (true)
        {
            try
            {
                commandText = Console.GetInputLine("> ").TrimEnd();
                if (_disposed) break;

                if (string.IsNullOrWhiteSpace(commandText) && !HasBufferedText)
                    continue;

                if (commandText.EndsWith('\\'))
                {
                    WriteBuffer(commandText[..^1]);
                    continue;
                }
                else if (commandText.EndsWith("\\n"))
                {
                    WriteBufferLine(commandText[..^2]);
                    continue;
                }

                if (HasBufferedText) commandText = ConsumeBuffer() + commandText;
                cmd = InteractiveCommand.Parse(commandText);
                var ret = InvokeFunction(cmd.PrimaryCommand, cmd.Args);
                if (ret is not null) Console.PrintLine(ret);
            }
            catch (FormatException ex)
            {
                Console.PrintError($"Invalid command line: {ex.Message}");
                continue;
            }
            catch (Exception ex)
            {
                Console.PrintError($"ERR! {ex.Message}");
                continue;
            }
        }
    }

    private object? InvokeFunction(string name, string[] parameters)
    {
        if (!_functions.TryGetValue(name, out var fnInfo))
            throw new InvalidOperationException($"Function '{name}' not found.");

        (object instance, MethodInfo fn) = fnInfo;
        object?[] convertedParams = [.. fn.GetParameters().Select((p, i) =>
        {
            if (i >= parameters.Length)
            {
                if (p.HasDefaultValue)
                    return p.DefaultValue;
                throw new ArgumentException($"Missing parameter '{p.Name}' for function '{name}'.");
            }
            try
            {
                var paramterAttr = p.GetCustomAttribute<InteractiveParameterAttribute>();
                var converter = paramterAttr?.Converter ?? ParameterConverters.FromTypeOf(p.ParameterType);
                return converter?.Invoke(parameters[i]) ?? Convert.ChangeType(parameters[i], p.ParameterType);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to convert parameter '{p.Name}' to type '{p.ParameterType.Name}': {ex.Message}");
            }
        })];

        return fn.Invoke(instance, convertedParams);
    }

    public InteractiveInterpreter RegisterFunction<T>(T instance) where T : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        var methods = instance
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<InteractiveFunctionAttribute>() != null);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<InteractiveFunctionAttribute>();
            _functions[attr?.Name ?? method.Name] = (instance, method);
        }

        return this;
    }

    public InteractiveInterpreter RegisterFunction<T>() where T : class, new() => RegisterFunction(new T());

    private void WriteBufferLine(string line)
    {
        _buffer.AppendLine(line);
    }

    private void WriteBuffer(string text)
    {
        _buffer.Append(text);
    }

    private string ConsumeBuffer()
    {
        var result = _buffer.ToString();
        _buffer.Clear();
        return result;
    }

    [InteractiveFunction(Description = "退出交互控制台")]
    public void Exit()
    {
        Environment.Exit(0);
    }

    [InteractiveFunction(Description = "查看所有可用的命令")]
    public void Help()
    {
        Console.PrintLine("Available commands:");
        foreach (var fn in _functions)
        {
            Console.Print($"- {fn.Key}");
            var parameters = fn.Value.Item2.GetParameters();
            if (parameters.Length > 0)
            {
                Console.Print(" ");
                Console.Print(string.Join(" ", parameters.Select(p =>
                {
                    return p.IsOptional ? $"[{p.Name}]" : $"<{p.Name}>";
                })));

                Console.Print('\n');
                foreach (var p in parameters)
                {
                    var paramAttr = p.GetCustomAttribute<InteractiveParameterAttribute>();
                    Console.Print($"    · {p.Name}: {p.ParameterType.Name}");

                    if (p.IsOptional)
                        Console.Print(" (Optional)");
                    if (p.HasDefaultValue)
                        Console.Print($" (Default = {p.DefaultValue ?? "null"})");

                    Console.Print('\n');
                    Console.PrintLow($"      {paramAttr?.Description ?? ""}");
                }
            }
            var attr = fn.Value.Item2.GetCustomAttribute<InteractiveFunctionAttribute>();
            if (attr?.Description is not null)
            {
                Console.PrintLine($": {attr.Description}");
            }

            Console.NextLine();
        }
    }
}
