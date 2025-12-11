using MoiraAlertPostprocessor.Infrastructure.NlServices;
using MoiraAlertPostprocessor.Infrastructure.Repositories.Interfaces;

namespace MoiraPostprocessor.Application.UseCases.ProcessAlert;

public class ProcessAlertUseCase(IAlertRepository alertRepository, INlpService nlpService)
{
    public async Task<ProcessAlertResponse> ExecuteAsync(ProcessAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        await alertRepository.SaveAsync(request.Alert, cancellationToken);

        var suggestion = await nlpService.GetSuggestionAsync(request.Alert, cancellationToken);

        return new ProcessAlertResponse(suggestion);
    }
}