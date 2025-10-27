using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos
{
    /// <summary>
    /// Main moira alert dto
    /// </summary>
    public record MoiraWebhookPayload
    {
        /// <summary>
        /// Trigger
        /// </summary>
        [JsonPropertyName("trigger")]
        public MoiraTriggerData Trigger { get; set; } = new();

        /// <summary>
        /// List of metric events with state changes
        /// </summary>
        [JsonPropertyName("events")]
        public List<MoiraEventData> Events { get; set; } = new();

        /// <summary>
        /// Contact/recipient info
        /// </summary>
        [JsonPropertyName("contact")]
        public MoiraContactData Contact { get; set; } = new();

        /// <summary>
        /// Base64 encoded plot image (legacy, single plot)
        /// </summary>
        [JsonPropertyName("plot")]
        public string? Plot { get; set; }

        /// <summary>
        /// Array of base64 encoded plot images
        /// </summary>
        [JsonPropertyName("plots")]
        public List<string>? Plots { get; set; }

        /// <summary>
        /// Whether alert was throttled/suppressed
        /// </summary>
        [JsonPropertyName("throttled")]
        public bool Throttled { get; set; }
    }
}
