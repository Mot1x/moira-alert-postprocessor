using MoiraAlertPostprocessor.Core.Domain.Entities;
using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices;

public interface INlpService
{
    Task<Suggestion> GetSuggestionAsync(MoiraAlert alert, CancellationToken cancellationToken = default);
}