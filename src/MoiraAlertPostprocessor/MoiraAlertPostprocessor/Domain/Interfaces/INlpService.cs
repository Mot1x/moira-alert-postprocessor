using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Domain.Interfaces;

public interface INlpService
{
    Task<Suggestion> GetSuggestionAsync(MoiraAlert alert, CancellationToken cancellationToken = default);
}