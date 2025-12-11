using MoiraAlertPostprocessor.Core.Domain.Entities;
using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraPostprocessor.Application.UseCases.ProcessAlert;

public class ProcessAlertResponse(Suggestion suggestion)
{
    public Suggestion Suggestion { get; } = suggestion;
}