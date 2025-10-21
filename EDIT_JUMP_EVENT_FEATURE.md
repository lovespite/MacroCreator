# 编辑跳转事件功能实现说明

## 概述
实现了在 `InsertJumpForm` 中编辑现有跳转事件的功能，用户现在可以右键点击跳转事件并选择"编辑跳转事件"来修改现有的跳转事件配置。

## 主要更改

### 1. InsertJumpForm.cs 增强

#### 新增字段
- `isEditMode`: 标识当前是否处于编辑模式
- `originalEventName`: 保存原始事件名称，用于名称验证时允许保留原名

#### 新增构造函数
```csharp
public InsertJumpForm(RecordedEvent existingEvent)
```
- 接受现有的 `RecordedEvent` 对象
- 自动设置为编辑模式
- 调用 `LoadEventData` 方法加载事件数据
- 更新窗体标题为"编辑跳转事件"

#### 新增方法：LoadEventData
```csharp
private void LoadEventData(RecordedEvent @event)
```
负责将现有事件的数据加载到表单控件中：
- **BreakEvent**: 选中"中断"单选按钮
- **JumpEvent**: 选中"无条件跳转"并填充目标事件名称
- **ConditionalJumpEvent**: 
  - 选中"条件跳转"
  - 根据条件类型加载像素检查或自定义表达式
  - 加载真/假分支的目标配置（事件名称或文件路径）

#### 修改：BtnOK_Click 验证逻辑
- 在编辑模式下，如果事件名称未改变，允许保留原名称（不触发重复名称错误）
- 只有当名称改变时才进行重复性检查

### 2. MainForm.cs 新增功能

#### 新增事件处理器
```csharp
private void ContextMenuStripEvents_Opening(object sender, CancelEventArgs e)
```
- 动态控制"编辑跳转事件"菜单项的可见性
- 只有选中单个跳转相关事件（JumpEvent、ConditionalJumpEvent、BreakEvent）时才显示

```csharp
private void EditJumpEventToolStripMenuItem_Click(object sender, EventArgs e)
```
- 验证选中的事件类型
- 创建编辑模式的 `InsertJumpForm`
- 处理事件更新：替换原事件并保留时间戳
- 刷新事件列表

### 3. MainForm.Designer.cs UI 更新

#### 新增控件
- `editJumpEventToolStripMenuItem`: 右键菜单中的"编辑跳转事件(&E)"选项

#### 修改的控件
- `contextMenuStripEvents`: 添加了新的菜单项和 `Opening` 事件处理器

## 使用方法

### 编辑现有跳转事件
1. 在事件列表中选中一个跳转事件（JumpEvent、ConditionalJumpEvent 或 BreakEvent）
2. 右键点击该事件
3. 选择"编辑跳转事件(&E)"
4. 在弹出的窗体中修改事件配置
5. 点击"确定"保存更改

### 支持的编辑内容
- **事件名称**: 可以修改或清空
- **中断事件**: 无额外配置
- **无条件跳转**: 可修改目标事件名称
- **条件跳转**: 
  - 条件类型（像素颜色检查/自定义表达式）
  - 像素检查的坐标、颜色和容差
  - 自定义条件表达式
  - 真/假分支的目标（事件名称或文件路径）

## 技术要点

1. **数据保留**: 编辑时保留原事件的 `TimeSinceLastEvent` 属性
2. **名称验证**: 智能处理名称验证，编辑时允许保留原名称
3. **UI 状态**: 根据选中事件类型动态显示/隐藏菜单项
4. **事件更新**: 通过索引直接替换事件，保持序列完整性

## 未来改进建议

1. 支持批量编辑相似的跳转事件
2. 添加编辑历史记录（撤销/重做）
3. 提供更多验证提示（如目标事件是否存在）
4. 支持复制事件配置
