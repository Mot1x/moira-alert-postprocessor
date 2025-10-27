using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos
{
    /// <summary>
    /// Contact/notification recipient details
    /// </summary>
    public record MoiraContactData
    {
        /// <summary>
        /// Contact type: webhook, email, slack, etc
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Contact value (URL for webhook, email for mail, etc)
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Unique contact identifier
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Owner username
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// Team name if contact is team-owned
        /// </summary>
        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;
    }
}
