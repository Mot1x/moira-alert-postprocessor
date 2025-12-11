using System.Collections.Concurrent;
using MoiraAlertPostprocessor.Domain.Entities;
using MoiraAlertPostprocessor.Infrastructure.Repositories.Interfaces;

namespace MoiraAlertPostprocessor.Infrastructure.Repositories;

public class InMemoryAlertRepository : IAlertRepository
{
    private readonly ConcurrentDictionary<string, MoiraAlert> store = new();

    public Task SaveAsync(MoiraAlert alert, CancellationToken cancellationToken = default)
    {
        var id = alert?.Trigger?.Id ?? Guid.NewGuid().ToString();
        store[id] = alert;
        return Task.CompletedTask;
    }

    public MoiraAlert? Get(string id)
    {
        return store.GetValueOrDefault(id);
    }
}