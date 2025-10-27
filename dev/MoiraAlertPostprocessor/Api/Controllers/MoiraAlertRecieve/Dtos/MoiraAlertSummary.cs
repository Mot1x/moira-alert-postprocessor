using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos
{
    /// <summary>
    /// Alert summary for recent alerts list (generated internally, not from Moira)
    /// </summary>
    public record MoiraAlertSummary
    {
        /// <summary>
        /// Unique alert ID (generated)
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Source trigger ID
        /// </summary>
        [JsonPropertyName("trigger_id")]
        public string TriggerId { get; set; } = string.Empty;

        /// <summary>
        /// Source trigger name
        /// </summary>
        [JsonPropertyName("trigger_name")]
        public string TriggerName { get; set; } = string.Empty;

        /// <summary>
        /// When alert was received
        /// </summary>
        [JsonPropertyName("time_stamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Number of events in alert
        /// </summary>
        [JsonPropertyName("event_count")]
        public int EventCount { get; set; }

        /// <summary>
        /// List of states from events
        /// </summary>
        [JsonPropertyName("states")]
        public List<string> States { get; set; } = new();

        /// <summary>
        /// List of trigger tags
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Whether alert was throttled
        /// </summary>
        [JsonPropertyName("throttled")]
        public bool Throttled { get; set; }
    }
}
