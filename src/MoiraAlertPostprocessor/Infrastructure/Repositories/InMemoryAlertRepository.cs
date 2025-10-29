using System.Collections.Concurrent;
using MoiraAlertPostprocessor.Domain.Entities;
using MoiraAlertPostprocessor.Domain.Interfaces;

namespace MoiraAlertPostprocessor.Infrastructure.Repositories;

public class InMemoryAlertRepository : IAlertRepository
{
    private readonly ConcurrentDictionary<string, MoiraAlert> store = new();

    public Task SaveAsync(MoiraAlert alert, CancellationToken cancellationToken = default)
    {
        // use trigger id + timestamp as key if present
        var id = alert?.Trigger?.Id ?? Guid.NewGuid().ToString();
        store[id] = alert;
        return Task.CompletedTask;
    }

    // helper for debugging
    public MoiraAlert Get(string id)
    {
        return store.TryGetValue(id, out var a) ? a : null;
    }
}