# MacroCreator - 自动化宏工具

一个基于 C# 和 Windows Forms 开发的强大宏录制与回放工具，支持键盘、鼠标操作的自动化，并提供高级条件判断功能。

## ✨ 特性

- 🎯 **全局钩子录制**：使用 Windows API 实现全局键盘和鼠标事件监听
- ⚡ **实时回放**：精确重现录制的操作序列，包括准确的时间间隔
- 🔍 **像素颜色检测**：支持基于屏幕像素颜色的条件判断
- � **流程控制** 🆕：支持无条件跳转和条件跳转，实现循环、分支等复杂逻辑
- �📁 **文件管理**：支持保存/加载宏序列，使用 XML 格式存储
- � **条件跳转**：根据像素颜色检测结果执行不同的宏文件
- ⌨️ **快捷键操作**：F9 录制、F10 播放、F11 停止
- 🎨 **用户友好界面**：直观的 Windows Forms 界面，支持事件列表查看和编辑
- 🆕 **现代化API**：使用 SendInput API 替代过时的 Win32 函数，提供更好的兼容性和稳定性

## 🚀 快速开始

### 系统要求

- Windows 操作系统
- .NET 9.0 或更高版本
- Visual Studio 2022 或 VS Code（用于开发）

### 安装运行

1. 克隆仓库：
```bash
git clone https://github.com/lovespite/MacroCreator.git
cd MacroCreator
```

2. 使用 Visual Studio 打开解决方案：
```bash
start MacroCreator.sln
```

3. 构建并运行：
   - 按 `F5` 或点击"开始调试"
   - 或者使用命令行：
```bash
dotnet build
dotnet run --project MacroCreator
```

## 📖 使用指南

### 基本操作

#### 录制宏
1. 点击 **"录制 (F9)"** 按钮或按下 `F9`
2. 执行需要录制的键盘和鼠标操作
3. 按下 `F11` 停止录制

#### 播放宏
1. 点击 **"播放 (F10)"** 按钮或按下 `F10`
2. 宏将自动重现之前录制的操作
3. 按下 `F11` 可随时停止播放

#### 文件操作
- **新建**：`Ctrl+N` 或菜单 → 文件 → 新建
- **打开**：`Ctrl+O` 或菜单 → 文件 → 打开
- **保存**：`Ctrl+S` 或菜单 → 文件 → 保存
- **另存为**：菜单 → 文件 → 另存为

### 高级功能

#### 条件判断
1. 在菜单栏选择 **编辑 → 插入条件判断**
2. 使用颜色拾取工具选择目标像素点：
   - 移动鼠标到目标位置
   - 按下 `Ctrl` 键拾取颜色和坐标
   - 或按 `F8` 进入拾取模式
3. 设置匹配和不匹配时要执行的宏文件
4. 点击确定插入条件判断事件

#### 流程控制 🆕
MacroCreator 支持强大的流程控制功能，允许在宏执行过程中进行跳转和条件跳转：

##### 无条件跳转
1. 在菜单栏选择 **编辑 → 插入跳转事件**
2. 选择 **"无条件跳转"**
3. 设置目标事件索引（要跳转到的事件编号）
4. 可选择性添加标签便于识别

##### 条件跳转
1. 在菜单栏选择 **编辑 → 插入跳转事件**
2. 选择 **"条件跳转"**
3. 选择条件类型：
   - **像素颜色检查**：检查指定坐标的像素颜色
   - **自定义条件**：使用表达式（支持时间、随机数等）
4. 设置条件为真和为假时的跳转目标
5. 条件为假时可选择继续执行下一个事件

##### 条件表达式示例
- `hour >= 9 && hour <= 17`：工作时间检查
- `random > 0.5`：50% 概率执行
- `true` / `false`：固定条件

##### 流程控制应用场景
- **循环执行**：创建重复执行的宏序列
- **错误处理**：根据执行结果选择不同的处理路径
- **时间控制**：根据时间条件执行不同操作
- **随机化**：为宏添加随机性，避免过于机械化

#### 快捷键列表
- `F9`：开始录制
- `F10`：开始播放  
- `F11`：停止当前操作
- `F8`：颜色拾取模式（在条件判断对话框中）
- `Delete`：删除选中的事件（在事件列表中）

## 🏗️ 项目结构

```
MacroCreator/
├── Controller/           # MVC 控制器层
│   └── MacroController.cs
├── Forms/               # Windows Forms 界面
│   ├── MainForm.cs      # 主窗体
│   ├── InsertConditionForm.cs  # 条件判断对话框
│   └── InsertJumpForm.cs       # 跳转事件对话框 🆕
├── Models/              # 数据模型
│   ├── AppState.cs      # 应用状态枚举
│   ├── RecordedEvent.cs # 事件基类
│   ├── MouseEvent.cs    # 鼠标事件
│   ├── KeyboardEvent.cs # 键盘事件
│   ├── DelayEvent.cs    # 延迟事件
│   ├── PixelConditionEvent.cs  # 像素条件事件
│   ├── JumpEvent.cs     # 跳转事件 🆕
│   ├── ConditionalJumpEvent.cs # 条件跳转事件 🆕
│   └── ...
├── Services/            # 业务逻辑服务
│   ├── RecordingService.cs     # 录制服务
│   ├── PlaybackService.cs      # 回放服务
│   ├── FileService.cs          # 文件服务
│   ├── PlaybackContext.cs      # 回放上下文
│   ├── JumpEventPlayer.cs      # 跳转事件播放器 🆕
│   ├── ConditionalJumpEventPlayer.cs # 条件跳转播放器 🆕
│   └── ...
├── Native/              # Windows API 封装
│   ├── NativeMethods.cs # P/Invoke 声明
│   └── InputHook.cs     # 全局钩子实现
├── Examples/            # 示例代码
│   ├── OptimizedPlaybackExample.cs # 优化回放示例
│   └── FlowControlExample.cs        # 流程控制示例 🆕
├── *EventPlayer.cs      # 事件播放器实现
└── Program.cs           # 程序入口点
```

## 🔧 技术实现

### 核心技术
- **全局钩子**：使用 `SetWindowsHookEx` API 实现全局键盘鼠标监听
- **事件系统**：基于观察者模式的事件驱动架构
- **策略模式**：不同类型事件使用对应的播放器处理
- **XML 序列化**：使用 `XmlSerializer` 进行宏序列的持久化

### 设计模式
- **MVC 架构**：清晰的分层设计，UI、业务逻辑和数据分离
- **策略模式**：`IEventPlayer` 接口支持不同类型事件的处理
- **观察者模式**：事件驱动的状态更新和 UI 刷新
- **外观模式**：`MacroController` 作为 UI 和后端服务的统一接口

### Windows API 使用
```csharp
// 现代化输入模拟 (已更新)
SendInput(), SetCursorPos()

// 全局钩子
SetWindowsHookEx(), UnhookWindowsHookEx()

// 屏幕颜色检测  
GetDC(), GetPixel(), ReleaseDC()
```

### API 现代化升级
本项目已从过时的 Win32 API 升级到现代化的输入模拟方案：
- ✅ **SendInput API**：替代了已弃用的 `mouse_event` 和 `keybd_event`
- ✅ **更好的兼容性**：在现代 Windows 版本上表现更稳定
- ✅ **原子化操作**：支持批量输入事件的原子化执行
- ✅ **向后兼容**：保持所有原有功能，用户体验无变化

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

### 开发环境设置
1. 安装 Visual Studio 2022 或更高版本
2. 确保安装了 .NET 9.0 SDK
3. 克隆项目并打开解决方案文件

### 代码规范
- 遵循 C# 编码约定
- 使用中文注释和变量名（项目特色）
- 保持代码整洁和良好的分层结构

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 🐛 问题反馈

如果您遇到问题或有改进建议，请通过以下方式联系：
- 提交 [GitHub Issue](https://github.com/lovespite/MacroCreator/issues)
- 发起 Pull Request

## � 更新日志

### v1.1.0 (2025-10-17)
- 🔄 **API 现代化升级**：将输入模拟从过时的 `mouse_event`/`keybd_event` 升级到现代的 `SendInput` API
- ✅ **兼容性提升**：提高在 Windows 10/11 系统上的稳定性和兼容性
- 🏗️ **代码重构**：优化 MouseEventPlayer 和 KeyboardEventPlayer 实现
- 📚 **文档更新**：更新技术说明和 API 使用文档

## �🔮 发展计划

- [ ] 支持更多条件判断类型（文本识别、图像匹配等）
- [ ] 添加宏编辑器，支持手动修改事件参数
- [ ] 支持循环和分支控制结构
- [ ] 添加宏运行统计和日志功能
- [ ] 支持热键全局触发
- [ ] 跨平台支持（Linux/macOS）

---

**MacroCreator** - 让重复操作变得简单！ 🎯