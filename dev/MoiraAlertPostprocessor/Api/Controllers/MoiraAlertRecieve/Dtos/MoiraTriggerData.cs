using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Api.Controllers.MoiraAlertRecieve.Dtos
{
    /// <summary>
    /// Trigger information: id, name, description, and tags
    /// </summary>
    public record MoiraTriggerData
    {
        /// <summary>
        /// Unique trigger identifier
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Trigger display name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Trigger description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// List of tags for grouping/filtering
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();
    }
}
