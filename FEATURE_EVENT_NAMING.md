# 事件命名功能更新

## 概述

实现了事件命名功能，允许用户为事件命名并通过名称跳转，而不仅仅是通过序号。

## 主要特性

### 1. 事件命名
- 所有事件现在都支持可选的名称属性 (`EventName`)
- 默认情况下事件是匿名的（`EventName` 为 null）
- 事件名称只能包含英文字母和数字（ASCII字符）
- 名称在事件序列中必须唯一

### 2. 按名称跳转
- **无条件跳转 (JumpEvent)**: 
  - 新增 `TargetEventName` 属性
  - 如果指定了目标事件名称，优先使用名称查找
  - 如果未找到名称或目标是匿名事件，跳转将被跳过（继续执行下一个事件）
  - 保留 `TargetIndex` 用于向后兼容

- **条件跳转 (ConditionalJumpEvent)**:
  - 新增 `TrueTargetEventName` 和 `FalseTargetEventName` 属性
  - 真假分支都支持按名称跳转
  - 同样的查找逻辑：名称优先，找不到则跳过
  - 保留索引属性用于向后兼容

### 3. 无法跳转匿名事件
- 按设计，如果跳转目标指定了事件名称但该事件不存在或为匿名，跳转将失败
- 这确保了只有明确命名的事件才能作为跳转目标
- 索引跳转仍然有效（向后兼容）

## 技术实现

### 模型变更

#### RecordedEvent.cs
```csharp
// 新增属性
public string? EventName { get; set; }

// 新增验证方法
public static bool IsValidEventName(string? name)
```

#### JumpEvent.cs
```csharp
// 新增属性
public string? TargetEventName { get; set; }

// 保留向后兼容
[Obsolete("使用 TargetEventName 代替")]
public string? Label { get; set; }
```

#### ConditionalJumpEvent.cs
```csharp
// 新增属性
public string? TrueTargetEventName { get; set; }
public string? FalseTargetEventName { get; set; }

// 保留向后兼容
[Obsolete("使用 TrueTargetEventName 代替")]
public string? TrueLabel { get; set; }
[Obsolete("使用 FalseTargetEventName 代替")]
public string? FalseLabel { get; set; }
```

### 服务层变更

#### PlaybackContext.cs
- 新增 `Events` 属性：事件序列的只读列表
- 新增 `FindEventIndexByName()` 方法：根据名称查找事件索引

#### JumpEventPlayer.cs
- 实现名称查找逻辑
- 优先使用 `TargetEventName`，找不到则使用 `TargetIndex`
- 如果指定了名称但未找到，返回 `Continue()` 而不是 `Jump()`

#### ConditionalJumpEventPlayer.cs
- 为真假分支实现名称查找逻辑
- 查找失败时返回 `Continue()`

#### PlaybackService.cs
- 更新 `Play()` 方法，将事件列表传递给 `PlaybackContext`

### UI 变更

#### MainForm.cs
- 更新 `RefreshEventList()`：在序号列显示事件名称（格式：`序号 [名称]`）
- 新增右键菜单项：**重命名事件**
- 新增 `RenameEventToolStripMenuItem_Click()` 事件处理器
  - 提供对话框供用户输入/修改事件名称
  - 验证名称格式（仅英文字母和数字）
  - 验证名称唯一性
  - 支持设置为空（匿名）

#### InsertJumpForm.cs
- 更新 `BtnOK_Click()`：
  - 为无条件跳转读取目标事件名称 (`txtTargetLabel`)
  - 为条件跳转读取真假分支目标事件名称 (`txtTrueLabel`, `txtFalseLabel`)
  - 验证所有输入的事件名称格式
  - 创建跳转事件时使用 `TargetEventName` 而不是 `Label`

#### InsertJumpForm.Designer.cs
- 更新标签文本和占位符提示
- `lblTargetLabel`: "目标事件名称"
- `txtTargetLabel.PlaceholderText`: "留空表示匿名事件（使用索引）"
- `txtTrueLabel.PlaceholderText`: "事件名称 (可选)"
- `txtFalseLabel.PlaceholderText`: "事件名称 (可选)"

## 使用示例

### 1. 命名事件
1. 在主窗口事件列表中右键点击事件
2. 选择"重命名事件"
3. 输入名称（如 "StartLoop", "CheckPixel", "EndLoop"）
4. 留空则设为匿名

### 2. 创建按名称跳转的无条件跳转
1. 点击"编辑" → "插入跳转"
2. 选择"无条件跳转"
3. 在"目标事件名称"框中输入目标事件的名称（如 "StartLoop"）
4. 或者保持为空并使用序号跳转

### 3. 创建按名称跳转的条件跳转
1. 点击"编辑" → "插入跳转"
2. 选择"条件跳转"
3. 配置条件（像素颜色或自定义）
4. 在"真分支"的事件名称框输入名称
5. 可选：启用假分支并输入其目标事件名称

## 向后兼容性

- 现有的基于索引的跳转完全兼容
- 旧的 XML 文件可以正常加载（EventName 为 null）
- `Label` 属性被标记为 `[Obsolete]` 但仍然支持
- 新功能是可选的，不使用名称时行为与之前完全相同

## 约束和限制

1. **名称格式**: 只能使用英文字母和数字（A-Z, a-z, 0-9）
2. **名称唯一性**: 同一序列中不能有重复的事件名称
3. **匿名事件**: 无法通过名称跳转到匿名事件
4. **跳转失败**: 如果指定的事件名称不存在，跳转会失败（继续执行下一个事件）

## 测试建议

1. 创建一个简单的事件序列
2. 为几个事件命名（如 "Start", "Loop", "End"）
3. 插入按名称跳转的无条件跳转
4. 测试跳转是否正确执行
5. 尝试跳转到不存在的名称（应该跳过跳转）
6. 尝试跳转到匿名事件（应该跳过跳转）
7. 测试条件跳转的名称功能
8. 测试重命名功能（包括名称冲突检测）

## 未来改进方向

1. 在 InsertJumpForm 中提供下拉列表显示所有已命名事件
2. 支持中文事件名称（需要修改验证逻辑）
3. 在跳转失败时记录日志或显示警告
4. 添加"查找所有引用"功能，显示哪些跳转指向某个事件
5. 支持事件名称自动补全
