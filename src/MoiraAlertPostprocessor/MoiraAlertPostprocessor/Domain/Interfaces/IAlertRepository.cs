using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Domain.Interfaces;

public interface IAlertRepository
{
    Task SaveAsync(MoiraAlert alert, CancellationToken cancellationToken = default);
}