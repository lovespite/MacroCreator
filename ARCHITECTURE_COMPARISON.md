# PlaybackService 架构对比

## 重构前（异常控制流）

```
PlaybackService.Play()
    └─> 循环处理事件
        ├─> player.ExecuteAsync()
        │   ├─> JumpEventPlayer
        │   │   └─> throw SequenceJumpException(targetIndex) ❌
        │   ├─> BreakEventPlayer  
        │   │   └─> throw SequenceJumpException(-1) { IsBreak = true } ❌
        │   └─> ConditionalJumpEventPlayer
        │       └─> throw SequenceJumpException(targetIndex) ❌
        │
        └─> try-catch SequenceJumpException ❌
            ├─> if (ex.IsBreak) return
            ├─> if (context.HasJumpTarget) 跳转
            └─> 验证跳转目标

问题：
- 使用异常控制正常流程（反模式）
- 性能开销大
- 代码逻辑混乱
- 调试时异常噪音多
```

## 重构后（结果对象模式）

```
PlaybackService.Play()
    └─> 循环处理事件
        ├─> result = player.ExecuteAsync() ✅
        │   ├─> JumpEventPlayer
        │   │   └─> return PlaybackResult.Jump(targetIndex) ✅
        │   ├─> BreakEventPlayer  
        │   │   └─> return PlaybackResult.Break() ✅
        │   ├─> ConditionalJumpEventPlayer
        │   │   └─> return PlaybackResult.Jump/JumpToFile/Continue() ✅
        │   └─> 其他 EventPlayer
        │       └─> return PlaybackResult.Continue() ✅
        │
        └─> switch (result.Control) ✅
            ├─> Continue: currentIndex++
            ├─> Jump: currentIndex = targetIndex
            ├─> Break: return
            └─> JumpToFile: 加载新文件并 return

优势：
- 符合设计规范
- 性能更好
- 代码清晰易懂
- 易于扩展和测试
```

## 数据流对比

### 重构前
```
Event → Player → throw Exception → catch → 解析异常 → 更改控制流
                     ❌ 异常作为控制流
```

### 重构后
```
Event → Player → return Result → switch → 更改控制流
                     ✅ 返回值作为控制流
```

## 关键类图

```
┌─────────────────────┐
│ PlaybackControl     │ (枚举)
├─────────────────────┤
│ + Continue          │
│ + Jump              │
│ + Break             │
│ + JumpToFile        │
└─────────────────────┘

┌─────────────────────────────┐
│ PlaybackResult              │
├─────────────────────────────┤
│ + Control: PlaybackControl  │
│ + TargetIndex: int          │
│ + FilePath: string?         │
├─────────────────────────────┤
│ + Continue(): PlaybackResult│
│ + Jump(int): PlaybackResult │
│ + Break(): PlaybackResult   │
│ + JumpToFile(string): ...   │
└─────────────────────────────┘

┌─────────────────────────────┐
│ IEventPlayer                │
├─────────────────────────────┤
│ + ExecuteAsync(event, ctx)  │
│   : Task<PlaybackResult>    │
└─────────────────────────────┘
         △
         │ implements
         │
    ┌────┴────┬──────────┬─────────┐
    │         │          │         │
JumpEvent  BreakEvent  Mouse/   Conditional
  Player     Player    Keyboard    Jump
                       Players    Player
```

## 代码示例对比

### 重构前 - JumpEventPlayer
```csharp
public Task ExecuteAsync(RecordedEvent ev, PlaybackContext context)
{
    if (ev is JumpEvent jumpEvent)
    {
        context.SetJumpTarget(jumpEvent.TargetIndex);
        throw new SequenceJumpException(jumpEvent.TargetIndex); // ❌
    }
    return Task.CompletedTask;
}
```

### 重构后 - JumpEventPlayer
```csharp
public Task<PlaybackResult> ExecuteAsync(RecordedEvent ev, PlaybackContext context)
{
    if (ev is JumpEvent jumpEvent)
    {
        return Task.FromResult(PlaybackResult.Jump(jumpEvent.TargetIndex)); // ✅
    }
    return Task.FromResult(PlaybackResult.Continue());
}
```

---

### 重构前 - PlaybackService 主循环
```csharp
try
{
    await player.ExecuteAsync(ev, context);
    
    if (context.HasJumpTarget) // ❌ 需要检查上下文状态
    {
        var targetIndex = context.JumpTargetIndex;
        context.ClearJumpTarget();
        // 处理跳转...
    }
}
catch (SequenceJumpException ex) // ❌ 使用异常控制流程
{
    if (ex.IsBreak)
    {
        return;
    }
    // 复杂的异常处理逻辑...
}
```

### 重构后 - PlaybackService 主循环
```csharp
var result = await player.ExecuteAsync(ev, context); // ✅ 直接获取结果

switch (result.Control) // ✅ 清晰的控制流
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
