using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoiraPostprocessor.API.Models
{
    public class IncomingMoiraWebhookDto
    {
        [JsonPropertyName("trigger")]
        public TriggerDto Trigger { get; set; }

        [JsonPropertyName("events")]
        public List<EventDto> Events { get; set; }

        [JsonPropertyName("contact")]
        public ContactDto Contact { get; set; }

        [JsonPropertyName("plot")]
        public string Plot { get; set; }

        [JsonPropertyName("plots")]
        public List<string> Plots { get; set; }

        [JsonPropertyName("throttled")]
        public bool Throttled { get; set; }
    }

    public class TriggerDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }

    public class EventDto
    {
        [JsonPropertyName("metric")]
        public string Metric { get; set; }

        // values is an object with arbitrary keys (e.g. {"t1": 1})
        [JsonPropertyName("values")]
        public Dictionary<string, double> Values { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("trigger_event")]
        public bool TriggerEvent { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("old_state")]
        public string OldState { get; set; }
    }

    public class ContactDto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
        [JsonPropertyName("team")]
        public string Team { get; set; }
    }
}