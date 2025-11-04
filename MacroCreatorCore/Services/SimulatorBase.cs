/*
 * CH9329 输入模拟器 - 高级封装类
 * 
 * 提供简化的鼠标和键盘操作接口，基于 Ch9329Controller 实现。
 */

using MacroCreator.Models;
using MacroCreator.Utils;

namespace MacroCreator.Services;

public abstract class SimulatorBase : ISimulator
{
    public abstract string Name { get; }
    public abstract Task MouseMove(int dx, int dy);
    public abstract Task MouseMoveTo(int x, int y);
    public abstract Task MouseDown(MouseButton button);
    public abstract Task MouseUp(MouseButton button);
    public virtual async Task MouseClick(MouseButton button, int delayMs = 25)
    {
        await MouseDown(button);
        if (delayMs > 0) await Task.Delay(delayMs);
        await MouseUp(button);
    }
    public virtual async Task MouseDoubleClick(MouseButton button, int delayMs = 50)
    {
        await MouseClick(button, delayMs);
        if (delayMs > 0) await Task.Delay(delayMs);
        await MouseClick(button, delayMs);
    }
    public abstract Task MouseWheel(int amount);
    public abstract Task KeyDown(params Keys[] key);
    public abstract Task KeyUp(params Keys[] key);

    public virtual async Task KeyPress(Keys key, int delayMs = 20)
    {
        await KeyDown(key);
        if (delayMs > 0) await Task.Delay(delayMs);
        await KeyUp(key);
    }

    public virtual async Task TypeText(string text, int delayMs = 20)
    {
        if (string.IsNullOrEmpty(text)) return;

        foreach (char c in text)
        {
            (KeyModifier mod, Keys key) = c.ToModifierAndKey();
            if (mod == KeyModifier.None)
            {
                await KeyPress(key, delayMs / 2);
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
            else
            {
                await KeyCombination(mod, key);
            }
        }
    }

    [Obsolete("Use KeyCombination(KeyModifier modifier, Keys key) instead.")]
    public virtual async Task KeyCombination(params Keys[] keys)
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

    public virtual async Task KeyCombination(KeyModifier modifier, Keys key)
    {
        var mKeys = modifier.ToKeysArray();
        await KeyDown(mKeys);
        await Task.Delay(10);
        await KeyDown(key);

        await Task.Delay(25);
        await ReleaseAllKeys();
    }

    public abstract Task ReleaseAllKeys();
    public abstract Task ReleaseAllMouse();
    public abstract void SetScreenResolution(int width, int height);
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
