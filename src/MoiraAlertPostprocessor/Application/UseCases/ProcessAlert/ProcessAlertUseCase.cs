using System.Threading;
using System.Threading.Tasks;
using MoiraAlertPostprocessor.Domain.Interfaces;
using MoiraPostprocessor.Domain.Entities;

namespace MoiraPostprocessor.Application.UseCases.ProcessAlert
{
    public abstract class ProcessAlertUseCase(IAlertRepository alertRepository, INlpService nlpService)
    {
        public async Task<ProcessAlertResponse> ExecuteAsync(ProcessAlertRequest request, CancellationToken cancellationToken = default)
        {
            // persist incoming alert for audit/troubleshooting
            await alertRepository.SaveAsync(request.Alert, cancellationToken);

            // call NLP to get suggestion
            var suggestion = await nlpService.GetSuggestionAsync(request.Alert, cancellationToken);

            return new ProcessAlertResponse(suggestion);
        }
    }
}