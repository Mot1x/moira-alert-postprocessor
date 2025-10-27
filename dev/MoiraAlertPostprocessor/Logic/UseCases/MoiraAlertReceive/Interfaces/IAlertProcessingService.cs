using MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos;

namespace MoiraAlertPostprocessor.Logic.UseCases.MoiraAlertReceive.Interfaces
{
    /// <summary>
    /// Interface for service capable of processing moira alerts
    /// </summary>
    public interface IAlertProcessingService
    {
        /// <summary>
        /// Proccess the alert
        /// </summary>
        Task ProcessAlertAsync(MoiraWebhookPayload payload);

        /// <summary>
        /// Get all recent Alerts
        /// </summary>
        Task<IEnumerable<MoiraAlertSummary>> GetRecentAlertsAsync(int count = 10);
    }
}
