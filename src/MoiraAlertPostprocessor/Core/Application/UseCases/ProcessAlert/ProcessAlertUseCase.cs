using System.Threading;
using System.Threading.Tasks;
using MoiraAlertPostprocessor.Infrastructure.NlServices;
using MoiraAlertPostprocessor.Infrastructure.Repositories.Interfaces;
using MoiraPostprocessor.Domain.Entities;

namespace MoiraPostprocessor.Application.UseCases.ProcessAlert
{
    public class ProcessAlertUseCase
    {
        private readonly IAlertRepository _alertRepository;
        private readonly INlpService _nlpService;

        public ProcessAlertUseCase(IAlertRepository alertRepository, INlpService nlpService)
        {
            _alertRepository = alertRepository;
            _nlpService = nlpService;
        }

        public async Task<ProcessAlertResponse> ExecuteAsync(ProcessAlertRequest request, CancellationToken cancellationToken = default)
        {
            await _alertRepository.SaveAsync(request.Alert, cancellationToken);

            var suggestion = await _nlpService.GetSuggestionAsync(request.Alert, cancellationToken);

            return new ProcessAlertResponse(suggestion);
        }
    }
}