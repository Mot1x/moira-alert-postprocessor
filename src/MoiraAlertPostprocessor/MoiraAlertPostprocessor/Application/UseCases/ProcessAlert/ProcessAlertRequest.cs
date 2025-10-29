using MoiraAlertPostprocessor.Domain.Entities;
using MoiraPostprocessor.Domain.Entities;

namespace MoiraPostprocessor.Application.UseCases.ProcessAlert
{
    public class ProcessAlertRequest(MoiraAlert alert)
    {
        public MoiraAlert Alert { get; } = alert;
    }
}