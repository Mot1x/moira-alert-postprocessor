using Microsoft.AspNetCore.Mvc;
using MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos;
using MoiraAlertPostprocessor.Logic.UseCases.MoiraAlertReceive.Interfaces;

namespace MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve
{
    /// <summary>
    /// Controller for receiving Moira Alerts
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MoiraAlertRecieveController : ControllerBase
    {
        private readonly ILogger<MoiraAlertRecieveController> _logger;
        private readonly IAlertProcessingService _alertProcessingService;

        public MoiraAlertRecieveController(ILogger<MoiraAlertRecieveController> logger, IAlertProcessingService alertProcessingService)
        {
            _logger = logger;
            _alertProcessingService = alertProcessingService;
        }

        /// <summary>
        /// Receives Moira webhook alerts and sends to further usage
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveAlert([FromBody] MoiraWebhookPayload payload)
        {
            try
            {
                _logger.LogInformation("Received Moira alert for trigger: {TriggerName} ({TriggerId})",
                    payload.Trigger.Name, payload.Trigger.Id);

                await _alertProcessingService.ProcessAlertAsync(payload);

                _logger.LogInformation("Alert processed successfully. Events count: {EventsCount}, Throttled: {Throttled}",
                    payload.Events.Count, payload.Throttled);

                return Ok(new { message = "Alert received and processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Moira alert");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new HealthCheckStatus() { Date = DateTime.UtcNow , Status = "Healthy" });
        }

        /// <summary>
        /// Gets recent alerts summary
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAlerts([FromQuery] int count = 10)
        {
            try
            {
                var alerts = await _alertProcessingService.GetRecentAlertsAsync(count);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent alerts");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}