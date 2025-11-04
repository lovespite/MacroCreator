// 命名空间定义了应用程序的入口点
using MacroCreator.Models;

namespace MacroCreator.Services;

public class NopPlayer : IEventPlayer
{
    public Task<PlaybackResult> ExecuteAsync(PlaybackContext _)
    { 
        return Task.FromResult(PlaybackResult.Continue());
    }
}

