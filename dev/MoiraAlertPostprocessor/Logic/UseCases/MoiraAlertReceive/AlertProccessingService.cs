using MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos;
using MoiraAlertPostprocessor.Logic.UseCases.MoiraAlertReceive.Interfaces;

namespace MoiraAlertPostprocessor.Logic.UseCases.MoiraAlertReceive
{
    /// <inheritdoc/>
    public class AlertProccessingService : IAlertProcessingService
    {
        private readonly ILogger<AlertProccessingService> _logger;
        private readonly List<MoiraAlertSummary> _recentAlerts = new();
        private readonly object _lock = new();

        public AlertProccessingService(ILogger<AlertProccessingService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task ProcessAlertAsync(MoiraWebhookPayload payload)
        {
            try
            {
                _logger.LogInformation("Processing alert for trigger: {TriggerName}", payload.Trigger.Name);

                var summary = new MoiraAlertSummary
                {
                    Id = Guid.NewGuid().ToString(),
                    TriggerId = payload.Trigger.Id,
                    TriggerName = payload.Trigger.Name,
                    Timestamp = DateTime.UtcNow,
                    EventCount = payload.Events.Count,
                    States = payload.Events.Select(e => e.State).Distinct().ToList(),
                    Tags = payload.Trigger.Tags,
                    Throttled = payload.Throttled
                };

                lock (_lock)
                {
                    _recentAlerts.Insert(0, summary);
                    
                    if (_recentAlerts.Count > 100)
                    {
                        _recentAlerts.RemoveRange(100, _recentAlerts.Count - 100);
                    }
                }

                _logger.LogInformation("Alert processed: {AlertId}, Trigger: {TriggerName}, Events: {EventCount}, States: {States}", 
                    summary.Id, summary.TriggerName, summary.EventCount, string.Join(",", summary.States));

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing alert for trigger: {TriggerName}", payload.Trigger.Name);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MoiraAlertSummary>> GetRecentAlertsAsync(int count = 10)
        {
            lock (_lock)
            {
                return _recentAlerts.Take(count).ToList();
            }
        }
    }
}
