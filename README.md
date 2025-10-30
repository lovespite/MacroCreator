# MacroCreator

一个功能强大的 Windows 自动化宏工具，支持录制、编辑和回放键盘鼠标操作，以及通过自定义脚本语言进行高级自动化控制。

## ✨ 核心功能

### 🎯 图形化界面 (MacroCreator)
- **录制和回放**：录制键盘和鼠标操作序列，精确回放
- **可视化编辑**：通过图形界面编辑、调整、重排事件序列
- **流程控制**：支持条件判断、循环、跳转等高级控制流
- **高精度计时**：使用高精度计时器确保事件回放的准确性
- **文件管理**：保存和加载宏序列为 XML 文件

### 📜 脚本语言 (MacroScript)
- **DSL 编译器**：自定义领域特定语言 (DSL)，用于编写复杂的自动化脚本
    - DSL语言支持见项目：[vscode-macroscript-support](https://github.com/lovespite/vscode-macroscript-support)
- **命令行工具**：编译、运行和管理宏脚本
- **条件控制**：支持 `if/else/endif`、`while/endwhile` 语法
- **像素检测**：通过 `PixelColor` 函数进行屏幕像素颜色判断
- **自定义表达式**：使用 `Custom()` 执行动态表达式
- **内联脚本**：使用 `Script()` 嵌入复杂逻辑

### 🔌 硬件重定向 (CH9329)
- **硬件模拟**：通过 CH9329 芯片模拟键盘鼠标输入
- **串口通信**：支持通过串口将输入重定向到外部设备
- **HID 设备控制**：实现 USB HID 设备级别的输入模拟

## 🚀 快速开始

### 前置要求
- Windows 操作系统
- .NET 9.0 或更高版本

### 使用图形界面

1. 运行 `MacroCreator.exe`
2. 点击 **开始录制** 按钮 (或按 F11)
3. 执行要录制的操作
4. 再次按 F11 停止录制
5. 点击 **播放** 按钮回放录制的操作

**命令行参数：**
```bash
# 打开/编辑宏文件
MacroCreator.exe open "path\to\macro.xml"
MacroCreator.exe edit "path\to\macro.xml"

# 直接运行宏文件（无界面）
MacroCreator.exe run "path\to\macro.xml"
```

### 使用脚本语言

**编译脚本：**
```bash
MacroScript.exe compile "script.macroscript" --output "output.xml"
```

**运行脚本：**
```bash
MacroScript.exe run "script.macroscript"
```

**重定向到硬件设备：**
```bash
MacroScript.exe run "script.macroscript" --redirect COM3
```

**其他选项：**
- `--pause` 或 `-p`：运行后暂停，等待用户按键
- `--hide`：运行时隐藏控制台窗口
- `--view` 或 `-v`：编译后在 MacroCreator 中查看

## 📖 脚本语言语法

### 基本事件

```macroscript
// 鼠标操作
MouseMoveTo(400, 400, 200)      // 移动到绝对坐标 (400, 400)，延迟 200ms
MouseMove(50, -50, 100)          // 相对移动 (50, -50)，延迟 100ms
MouseClick(Left, 100)            // 单击左键，延迟 100ms
MouseDown(Left, 50)              // 按下左键
MouseUp(Left, 20)                // 释放左键
MouseWheel(-100, 10)             // 滚动鼠标滚轮

// 键盘操作
KeyDown(LControlKey, 100)        // 按下左 Ctrl 键
KeyUp(LControlKey, 50)           // 释放左 Ctrl 键
KeyPress(LWin, R, 25)            // 按下并释放 Win+R 组合键

// 延迟
Delay(1000)                      // 暂停 1000ms
```

### 流程控制

```macroscript
// 条件判断
if (PixelColor(10, 20) == RGB(255, 0, 0, 5))
    KeyPress(LWin, R, 25)
endif

// if-else
if (Custom(`now().Hour > 9`))
    Delay(1000)
else
    Delay(500)
endif

// 不等于判断
if (PixelColor(0, 0) != RGB(0, 0, 0))
    MouseMoveTo(0, 0)
    exit
endif

// 循环
while (PixelColor(100, 100) == RGB(255, 255, 255))
    MouseWheel(-100, 10)
    
    if (Custom(`now().Hour >= 17`))
        break
    endif
endwhile

// 标签和跳转
label MyStartPoint
MouseMove(0, 0, 100)
goto MyStartPoint
```

### 内联脚本

```macroscript
Script(`
    // 操作全局变量
    set("MyVar", 0)
    set("MyVar", (int)get("MyVar") + 1)
    println("MyVar is now: " + get("MyVar"))
`, "MyScript")
```

### 像素颜色检测

```macroscript
// RGB(r, g, b, [tolerance])
// tolerance: 颜色容差 (默认为 0)

if (PixelColor(100, 200) == RGB(255, 0, 0, 5))
    // 如果坐标 (100, 200) 处的像素颜色接近红色
    // (容差范围 ±5)
    MouseClick(Left)
endif
```

## 🏗️ 项目结构

```
MacroCreator/
├── MacroCreator/              # 主 WinForms 应用程序
│   ├── Controller/
│   │   └── MacroController.cs # 核心控制器（外观模式）
│   ├── Forms/                 # 窗体界面
│   │   ├── MainForm.cs        # 主窗口
│   │   ├── EditKeyboardEventForm.cs
│   │   ├── EditMouseEventForm.cs
│   │   ├── EditDelayEventForm.cs
│   │   ├── EditScriptEventForm.cs
│   │   └── EditFlowControlEventForm.cs
│   ├── Models/                # 数据模型
│   │   ├── AppState.cs
│   │   ├── PlaybackConfiguration.cs
│   │   └── Events/            # 事件类型定义
│   │       ├── MacroEvent.cs  # 事件基类
│   │       ├── KeyboardEvent.cs
│   │       ├── MouseEvent.cs
│   │       ├── DelayEvent.cs
│   │       ├── ScriptEvent.cs
│   │       ├── JumpEvent.cs
│   │       ├── ConditionalJumpEvent.cs
│   │       └── BreakEvent.cs
│   ├── Services/              # 业务逻辑服务
│   │   ├── RecordingService.cs        # 录制服务
│   │   ├── PlaybackService.cs         # 回放服务
│   │   ├── PlaybackContext.cs         # 回放上下文
│   │   ├── FileService.cs             # 文件读写
│   │   ├── HighPrecisionTimer.cs      # 高精度计时器
│   │   ├── IEventPlayer.cs            # 事件播放器接口
│   │   ├── KeyboardEventPlayer.cs
│   │   ├── MouseEventPlayer.cs
│   │   ├── DelayEventPlayer.cs
│   │   ├── ScriptEventPlayer.cs
│   │   ├── JumpEventPlayer.cs
│   │   ├── ConditionalJumpEventPlayer.cs
│   │   ├── Ch9329KeyboardEventPlayer.cs
│   │   ├── Ch9329MouseEventPlayer.cs
│   │   └── CH9329/            # CH9329 硬件支持
│   │       ├── InputSimulator.cs
│   │       ├── Ch9329Controller.cs
│   │       ├── Ch9329Exception.cs
│   │       └── ...
│   ├── Native/                # Windows API 封装
│   │   ├── NativeMethods.cs   # P/Invoke 声明
│   │   └── InputHook.cs       # 键盘鼠标钩子
│   └── Utils/                 # 工具类
│
├── MacroScript/               # 命令行脚本工具
│   ├── Program.cs             # 命令行入口
│   ├── Dsl/                   # DSL 编译器
│   │   ├── Scripting.cs
│   │   ├── Lexer.cs           # 词法分析器
│   │   ├── NewDslParser.cs    # 语法分析器
│   │   └── Token.cs
│   └── *.macroscript          # 示例脚本文件
│
└── MacroScriptTests/          # 单元测试
```

## 🔧 技术特点

### 高精度计时
使用 `QueryPerformanceCounter` 实现毫秒级精确延迟，确保事件回放的准确性。

### 策略模式
通过 `IEventPlayer` 接口实现不同类型事件的播放器，支持灵活扩展。

### 外观模式
`MacroController` 作为外观类，协调各个服务之间的交互，简化 UI 层的调用。

### 硬件抽象
支持本地输入模拟和 CH9329 硬件重定向两种模式，运行时动态切换。

### 表达式引擎
使用 `DynamicExpresso` 库支持动态表达式求值，实现灵活的条件控制。

## 📦 依赖项

- **DynamicExpresso.Core** (2.19.3) - 动态表达式求值
- **System.IO.Ports** (9.0.10) - 串口通信

## 🎮 支持的按键和鼠标操作

### 鼠标按钮
- Left (左键)
- Right (右键)
- Middle (中键)
- X1, X2 (侧键)

### 键盘按键
支持所有标准 Windows 虚拟键码 (System.Windows.Forms.Keys)，包括：
- 字母键：A-Z
- 数字键：D0-D9, NumPad0-NumPad9
- 功能键：F1-F24
- 修饰键：Shift, Ctrl, Alt, Win
- 导航键：Home, End, PageUp, PageDown, Arrow Keys
- 等等...

运行 `MacroScript.exe showkeys` 查看所有支持的按键名称。

## 📝 文件格式

宏序列以 XML 格式保存，示例：

```xml
<MacroEventCollection>
  <MacroEvent Type="MouseEvent" Action="MoveTo" X="400" Y="400" TimeSinceLastEvent="200" />
  <MacroEvent Type="KeyboardEvent" Action="Down" Key="LControlKey" TimeSinceLastEvent="100" />
  <MacroEvent Type="KeyboardEvent" Action="Up" Key="LControlKey" TimeSinceLastEvent="50" />
  <MacroEvent Type="DelayEvent" DelayMilliseconds="1000" TimeSinceLastEvent="0" />
</MacroEventCollection>
```

## 🛡️ 注意事项

1. **管理员权限**：某些应用程序可能需要以管理员身份运行才能正常模拟输入
2. **硬件设备**：使用 CH9329 功能需要连接相应的硬件设备
3. **安全性**：自动化脚本可能会干扰系统操作，请谨慎使用
4. **循环控制**：避免创建无限循环，建议添加适当的退出条件

## 📄 许可证

本项目为个人开源项目。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📧 联系方式

如有问题或建议，请通过 GitHub Issues 反馈。

---

**MacroCreator** - 让自动化更简单！