# PlaybackService 架构重构说明

## 问题分析

原有的 `PlaybackService` 使用异常 (`SequenceJumpException`) 来控制程序流程，这违反了以下设计原则：

1. **异常应该用于异常情况**：异常应该用于处理错误和意外情况，而不是正常的控制流
2. **性能问题**：异常处理的性能开销较大，频繁抛出和捕获异常会影响性能
3. **代码可读性**：使用异常控制流程使代码逻辑不够清晰，难以理解和维护
4. **调试困难**：当开发者启用"在所有异常时中断"调试选项时，会被大量的正常控制流异常干扰

## 解决方案

采用**结果对象模式 (Result Object Pattern)** 来替代异常控制流：

### 1. 新增 `PlaybackControl` 枚举

定义了四种播放控制流类型：

```csharp
public enum PlaybackControl
{
    Continue,      // 继续执行下一个事件
    Jump,          // 跳转到指定索引
    Break,         // 中断并终止播放
    JumpToFile     // 跳转到外部文件
}
```

### 2. 新增 `PlaybackResult` 类

封装事件播放器的执行结果，包含控制流指令和相关数据：

```csharp
public class PlaybackResult
{
    public PlaybackControl Control { get; init; }
    public int TargetIndex { get; init; }
    public string? FilePath { get; init; }
    
    // 提供便捷的工厂方法
    public static PlaybackResult Continue();
    public static PlaybackResult Jump(int targetIndex);
    public static PlaybackResult Break();
    public static PlaybackResult JumpToFile(string filePath);
}
```

### 3. 更新 `IEventPlayer` 接口

修改返回类型从 `Task` 到 `Task<PlaybackResult>`：

```csharp
public interface IEventPlayer
{
    Task<PlaybackResult> ExecuteAsync(RecordedEvent ev, PlaybackContext context);
}
```

### 4. 重构 `PlaybackService` 主循环

移除 try-catch 异常处理，改用 switch 语句处理 `PlaybackResult`：

```csharp
var result = await player.ExecuteAsync(ev, context);

switch (result.Control)
{
    case PlaybackControl.Continue:
        currentIndex++;
        break;
        
    case PlaybackControl.Jump:
        currentIndex = result.TargetIndex;
        scheduledTime = _timer.GetPreciseMilliseconds();
        break;
        
    case PlaybackControl.Break:
        return;
        
    case PlaybackControl.JumpToFile:
        await loadAndPlayNewFileCallback(result.FilePath);
        return;
}
```

### 5. 更新所有 EventPlayer 实现

- **JumpEventPlayer**: 返回 `PlaybackResult.Jump(targetIndex)`
- **BreakEventPlayer**: 返回 `PlaybackResult.Break()`
- **ConditionalJumpEventPlayer**: 根据条件返回相应的 `PlaybackResult`
- **其他 EventPlayer**: 返回 `PlaybackResult.Continue()`

### 6. 清理工作

- 删除 `SequenceJumpException` 类
- 简化 `PlaybackContext`，移除不再需要的跳转相关方法 (`SetJumpTarget`, `ClearJumpTarget`, `HasJumpTarget`)

## 优势

1. **符合设计规范**：不再使用异常来控制正常流程
2. **性能提升**：避免了异常抛出和捕获的开销
3. **代码清晰**：控制流逻辑一目了然，使用 switch 语句明确表达意图
4. **易于扩展**：如需添加新的控制流类型，只需在枚举中添加新值
5. **易于测试**：可以直接测试 EventPlayer 的返回值，无需处理异常
6. **调试友好**：不会被正常控制流的异常干扰调试过程

## 修改的文件

### 新增文件
- `Models/PlaybackControl.cs` - 播放控制流枚举
- `Models/PlaybackResult.cs` - 播放结果类

### 修改的文件
- `Services/IEventPlayer.cs` - 修改接口签名
- `Services/PlaybackService.cs` - 重构主循环
- `Services/JumpEventPlayer.cs` - 返回 PlaybackResult
- `Services/BreakEventPlayer.cs` - 返回 PlaybackResult
- `Services/ConditionalJumpEventPlayer.cs` - 返回 PlaybackResult
- `Services/MouseEventPlayer.cs` - 返回 PlaybackResult
- `Services/KeyboardEventPlayer.cs` - 返回 PlaybackResult
- `Services/DelayEventPlayer.cs` - 返回 PlaybackResult
- `Services/PlaybackContext.cs` - 简化并移除跳转相关方法

### 删除的文件
- `Models/SequenceJumpException.cs` - 不再需要

## 向后兼容性

此重构改变了内部实现，但不影响对外的 API。`PlaybackService.Play()` 方法的签名保持不变，因此不会影响调用代码。
