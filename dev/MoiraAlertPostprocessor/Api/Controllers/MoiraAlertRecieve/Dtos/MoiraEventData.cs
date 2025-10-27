using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos
{
    /// <summary>
    /// Single metric event with values, state changes, and timestamp
    /// </summary>
    public record MoiraEventData
    {
        /// <summary>
        /// Metric name and labels
        /// </summary>
        [JsonPropertyName("metric")]
        public string Metric { get; set; } = string.Empty;

        /// <summary>
        /// Metric values per target (target -> value), can be null
        /// </summary>
        [JsonPropertyName("values")]
        public Dictionary<string, double>? Values { get; set; }

        /// <summary>
        /// Unix timestamp in seconds
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// True if final trigger state event, false for metric state change
        /// </summary>
        [JsonPropertyName("trigger_event")]
        public bool IsTriggerEvent { get; set; }

        /// <summary>
        /// Current state: OK, WARN, ERROR, NODATA
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Previous state: OK, WARN, ERROR, NODATA
        /// </summary>
        [JsonPropertyName("old_state")]
        public string OldState { get; set; } = string.Empty;
    }
}
