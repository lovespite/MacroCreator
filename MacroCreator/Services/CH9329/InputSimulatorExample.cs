/*
 * InputSimulator 使用示例
 * 
 * 展示如何使用 InputSimulator 类进行鼠标和键盘操作。
 */

namespace MacroCreator.Services.CH9329;

/// <summary>
/// InputSimulator 使用示例类。
/// </summary>
public static class InputSimulatorExample
{
  /// <summary>
  /// 基本鼠标操作示例。
  /// </summary>
  public static async Task MouseOperationsExample()
  {
    using var simulator = new InputSimulator("COM3", 9600);
    simulator.Open();

    // 设置屏幕分辨率
    simulator.SetScreenResolution(1920, 1080);

    // 相对移动鼠标
    await simulator.MouseMove(100, 50);
    await Task.Delay(100);

    // 移动到绝对位置
    await simulator.MouseMoveTo(960, 540); // 屏幕中心
    await Task.Delay(100);

    // 鼠标点击
    await simulator.MouseClick(MouseButton.Left);
    await Task.Delay(100);

    // 鼠标双击
    await simulator.MouseDoubleClick(MouseButton.Left);
    await Task.Delay(100);

    // 鼠标按下并拖动
    await simulator.MouseDown(MouseButton.Left);
    await simulator.MouseMove(100, 100);
    await simulator.MouseUp(MouseButton.Left);

    // 滚轮滚动
    await simulator.MouseWheel(3); // 向上滚动
    await Task.Delay(100);
    await simulator.MouseWheel(-3); // 向下滚动

    simulator.Close();
  }

  /// <summary>
  /// 基本键盘操作示例。
  /// </summary>
  public static async Task KeyboardOperationsExample()
  {
    using var simulator = new InputSimulator("COM3", 9600);
    simulator.Open();

    // 单个按键
    await simulator.KeyPress(Keys.A);
    await Task.Delay(100);

    // 输入文本
    await simulator.TypeText("Hello, World!");
    await Task.Delay(100);

    // 组合键 (Ctrl+C)
    await simulator.KeyCombination(Keys.Control, Keys.C);
    await Task.Delay(100);

    // 组合键 (Ctrl+Shift+Esc)
    await simulator.KeyCombination(Keys.Control, Keys.Shift, Keys.Escape);
    await Task.Delay(100);

    // 按下并保持按键
    await simulator.KeyDown(Keys.Shift);
    await simulator.KeyPress(Keys.A);
    await simulator.KeyPress(Keys.B);
    await simulator.KeyPress(Keys.C);
    await simulator.KeyUp(Keys.Shift);

    simulator.Close();
  }

  /// <summary>
  /// 复杂操作示例：模拟游戏操作。
  /// </summary>
  public static async Task GameOperationExample()
  {
    using var simulator = new InputSimulator("COM3", 9600);
    simulator.Open();
    simulator.SetScreenResolution(1920, 1080);

    // 移动到屏幕中心
    await simulator.MouseMoveTo(960, 540);
    await Task.Delay(100);

    // 模拟 WASD 移动
    await simulator.KeyDown(Keys.W); // 向前移动
    await Task.Delay(500);
    await simulator.KeyUp(Keys.W);

    // 边移动边转视角
    await simulator.KeyDown(Keys.D); // 向右移动
    await simulator.MouseMove(50, 0); // 右转
    await Task.Delay(300);
    await simulator.MouseMove(50, 0);
    await Task.Delay(300);
    await simulator.KeyUp(Keys.D);

    // 跳跃
    await simulator.KeyPress(Keys.Space);
    await Task.Delay(200);

    // 攻击（鼠标左键）
    await simulator.MouseClick(MouseButton.Left);
    await Task.Delay(100);

    // 瞄准射击
    await simulator.MouseDown(MouseButton.Right); // 瞄准
    await Task.Delay(500);
    await simulator.MouseClick(MouseButton.Left); // 射击
    await simulator.MouseUp(MouseButton.Right);

    simulator.Close();
  }

  /// <summary>
  /// 办公自动化示例：复制粘贴操作。
  /// </summary>
  public static async Task OfficeAutomationExample()
  {
    using var simulator = new InputSimulator("COM3", 9600);
    simulator.Open();

    // 选中全部文本 (Ctrl+A)
    await simulator.KeyCombination(Keys.Control, Keys.A);
    await Task.Delay(100);

    // 复制 (Ctrl+C)
    await simulator.KeyCombination(Keys.Control, Keys.C);
    await Task.Delay(100);

    // 新建文档 (Ctrl+N)
    await simulator.KeyCombination(Keys.Control, Keys.N);
    await Task.Delay(500);

    // 粘贴 (Ctrl+V)
    await simulator.KeyCombination(Keys.Control, Keys.V);
    await Task.Delay(100);

    // 保存 (Ctrl+S)
    await simulator.KeyCombination(Keys.Control, Keys.S);
    await Task.Delay(100);

    simulator.Close();
  }

  /// <summary>
  /// 绘图示例：在画图软件中绘制图形。
  /// </summary>
  public static async Task DrawingExample()
  {
    using var simulator = new InputSimulator("COM3", 9600);
    simulator.Open();
    simulator.SetScreenResolution(1920, 1080);

    // 移动到起点
    await simulator.MouseMoveTo(500, 500);
    await Task.Delay(100);

    // 按下鼠标左键开始绘制
    await simulator.MouseDown(MouseButton.Left);

    // 绘制正方形
    await simulator.MouseMove(200, 0);   // 右
    await Task.Delay(50);
    await simulator.MouseMove(0, 200);   // 下
    await Task.Delay(50);
    await simulator.MouseMove(-200, 0);  // 左
    await Task.Delay(50);
    await simulator.MouseMove(0, -200);  // 上
    await Task.Delay(50);

    // 释放鼠标左键
    await simulator.MouseUp(MouseButton.Left);

    simulator.Close();
  }

  /// <summary>
  /// 批量操作示例：填写表单。
  /// </summary>
  public static async Task FormFillingExample()
  {
    using var simulator = new InputSimulator("COM3", 9600);
    simulator.Open();

    // 点击第一个输入框
    await simulator.MouseMoveTo(500, 300);
    await simulator.MouseClick(MouseButton.Left);
    await Task.Delay(200);

    // 输入姓名
    await simulator.TypeText("Zhang San");
    await Task.Delay(100);

    // Tab 到下一个字段
    await simulator.KeyPress(Keys.Tab);
    await Task.Delay(200);

    // 输入邮箱
    await simulator.TypeText("zhangsan@example.com");
    await Task.Delay(100);

    // Tab 到下一个字段
    await simulator.KeyPress(Keys.Tab);
    await Task.Delay(200);

    // 输入电话
    await simulator.TypeText("13800138000");
    await Task.Delay(100);

    // 提交表单 (Enter)
    await simulator.KeyPress(Keys.Enter);

    simulator.Close();
  }

  /// <summary>
  /// 使用现有控制器实例的示例。
  /// </summary>
  public static async Task UseExistingControllerExample()
  {
    // 创建底层控制器
    using var controller = new Ch9329Controller("COM3", 9600);
    controller.Open();

    // 使用高级封装
    using var simulator = new InputSimulator(controller);

    // 执行操作
    await simulator.MouseClick(MouseButton.Left);
    await simulator.TypeText("Test");

    // 如果需要，还可以直接使用底层控制器
    var chipInfo = await controller.GetInfoAsync();
    Console.WriteLine($"芯片版本: {chipInfo.Version}, USB 状态: {chipInfo.UsbStatus}");

    controller.Close();
  }

  /// <summary>
  /// 错误处理示例。
  /// </summary>
  public static async Task ErrorHandlingExample()
  {
    InputSimulator? simulator = null;

    try
    {
      simulator = new InputSimulator("COM3", 9600);
      simulator.Open();

      await simulator.MouseClick(MouseButton.Left);
      await simulator.TypeText("Hello");
    }
    catch (CH9329Exception ex)
    {
      Console.WriteLine($"CH9329 错误: {ex.ErrorCode} - {ex.Message}");
    }
    catch (TimeoutException ex)
    {
      Console.WriteLine($"超时错误: {ex.Message}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"未知错误: {ex.Message}");
    }
    finally
    {
      simulator?.Close();
      simulator?.Dispose();
    }
  }
}
