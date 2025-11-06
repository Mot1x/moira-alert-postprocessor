using MoiraAlertPostprocessor.Domain.Entities;
using MoiraPostprocessor.Domain.Entities;

namespace MoiraPostprocessor.Application.UseCases.ProcessAlert
{
    public class ProcessAlertResponse(Suggestion suggestion)
    {
        public Suggestion Suggestion { get; } = suggestion;
    }
}