using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.Repositories.Interfaces;

public interface IAlertRepository
{
    Task SaveAsync(MoiraAlert alert, CancellationToken cancellationToken = default);
}