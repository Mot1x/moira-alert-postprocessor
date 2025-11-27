using System.Collections.Concurrent;

namespace MoiraAlertPostprocessor.Infrastructure.Repositories;

public interface IVoteRepository
{
    bool TryVote(long chatId, int messageId, long userId);
}

public class InMemoryVoteRepository : IVoteRepository
{
    private readonly ConcurrentDictionary<string, bool> votes = new();

    public bool TryVote(long chatId, int messageId, long userId)
    {
        var key = $"{chatId}:{messageId}:{userId}";
        return votes.TryAdd(key, true);
    }
}