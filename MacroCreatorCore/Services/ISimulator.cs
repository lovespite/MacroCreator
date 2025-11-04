/*
 * CH9329 输入模拟器 - 高级封装类
 * 
 * 提供简化的鼠标和键盘操作接口，基于 Ch9329Controller 实现。
 */
using MacroCreator.Models;
namespace MacroCreator.Services;

public interface ISimulator : IDisposable
{
    /// <summary>
    /// 模拟器名称 
    /// </summary>
    string Name { get; }
    /// <summary>
    /// 相对移动鼠标。
    /// </summary>
    /// <param name="dx">X 轴相对移动量 (-127 到 +127)。</param>
    /// <param name="dy">Y 轴相对移动量 (-127 到 +127)。</param>
    Task MouseMove(int dx, int dy);
    /// <summary>
    /// 移动鼠标到绝对坐标位置。
    /// </summary>
    /// <param name="x">目标 X 坐标（屏幕像素）。</param>
    /// <param name="y">目标 Y 坐标（屏幕像素）。</param>
    Task MouseMoveTo(int x, int y);
    /// <summary>
    /// 按下鼠标按键（不释放）。
    /// </summary>
    /// <param name="button">要按下的鼠标按键。</param>
    Task MouseDown(MouseButton button);
    /// <summary>
    /// 释放鼠标按键。
    /// </summary>
    /// <param name="button">要释放的鼠标按键。</param>
    Task MouseUp(MouseButton button);
    /// <summary>
    /// 点击鼠标按键（按下并释放）。
    /// </summary>
    /// <param name="button">要点击的鼠标按键。</param>
    /// <param name="delayMs">按下和释放之间的延迟（毫秒），默认 50ms。</param>
    Task MouseClick(MouseButton button, int delayMs = 50);
    /// <summary>
    /// 双击鼠标按键。
    /// </summary>
    /// <param name="button">要双击的鼠标按键。</param>
    /// <param name="delayMs">两次点击之间的延迟（毫秒），默认 100ms。</param>
    Task MouseDoubleClick(MouseButton button, int delayMs = 100);

    /// <summary>
    /// 鼠标滚轮滚动。
    /// </summary>
    /// <param name="amount">滚动量。正值向上，负值向下。</param>
    Task MouseWheel(int amount);
    /// <summary>
    /// 按下键盘按键（不释放）。
    /// </summary>
    /// <param name="key">要按下的按键。</param>
    Task KeyDown(Keys key);
    /// <summary>
    /// 释放键盘按键。
    /// </summary>
    /// <param name="key">要释放的按键。</param>
    Task KeyUp(Keys key);
    /// <summary>
    /// 按下并释放键盘按键。
    /// </summary>
    /// <param name="key">要按下的按键。</param>
    /// <param name="delayMs">按下和释放之间的延迟（毫秒），默认 50ms。</param>
    Task KeyPress(Keys key, int delayMs = 50);
    /// <summary>
    /// 输入文本（连续按键）,仅支持英文（包含大小写）、数字以及基础字符
    /// </summary>
    /// <param name="text">要输入的文本。</param>
    /// <param name="delayMs">每个按键之间的延迟（毫秒），默认 50ms。</param>
    Task TypeText(string text, int delayMs = 50);
    /// <summary>
    /// 按下组合键（例如 Ctrl+C）。
    /// </summary>
    /// <param name="keys">要按下的按键组合。</param>
    Task KeyCombination(params Keys[] keys);
    /// <summary>
    /// 释放所有键盘按键。
    /// </summary>
    Task ReleaseAllKeys();
    /// <summary>
    /// 释放所有鼠标按键。
    /// </summary>
    /// <returns></returns>
    Task ReleaseAllMouse();
    /// <summary>
    /// 设置屏幕分辨率（用于绝对坐标转换）。
    /// </summary>
    /// <param name="width">屏幕宽度（像素）。</param>
    /// <param name="height">屏幕高度（像素）。</param>
    void SetScreenResolution(int width, int height);
}
