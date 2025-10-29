using System.Text;
using System.Text.Json;
using MoiraAlertPostprocessor.Domain.Entities;
using MoiraAlertPostprocessor.Domain.Interfaces;

namespace MoiraAlertPostprocessor.Infrastructure.GptOss;

public class GptOssClient(HttpClient http, GptOssOptions options) : INlpService
{
    public async Task<Suggestion> GetSuggestionAsync(MoiraAlert alert, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(alert);

        var payload = new
        {
            model = options.Model ?? "gpt-oss-small",
            input = prompt
        };

        using var resp = await http.PostAsJsonAsync(options.Endpoint, payload, cancellationToken);
        resp.EnsureSuccessStatusCode();

        // Attempt to parse text field from response; adapt to real gpt-oss response shape.
        var doc = await resp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var text = ExtractText(doc);

        // simple splitting into summary/details/actions
        var summary = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "No summary";
        var details = text ?? string.Empty;
        var actions = ExtractActions(text);

        return new Suggestion(summary, details, actions);
    }

    private string BuildPrompt(MoiraAlert alert)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an observability assistant. Given the Moira alert below produce:");
        sb.AppendLine("1) Short summary of probable root cause.");
        sb.AppendLine("2) Detailed explanation.");
        sb.AppendLine("3) Concrete remediation steps (bullet list).");
        sb.AppendLine();
        sb.AppendLine("Trigger:");
        sb.AppendLine($"- Id: {alert.Trigger?.Id}");
        sb.AppendLine($"- Name: {alert.Trigger?.Name}");
        sb.AppendLine($"- Description: {alert.Trigger?.Description}");
        sb.AppendLine($"- Tags: {string.Join(", ", alert.Trigger?.Tags ?? Enumerable.Empty<string>())}");
        sb.AppendLine();

        sb.AppendLine("Events:");
        foreach (var ev in alert.Events)
        {
            sb.AppendLine($"- RawMetric: {ev.RawMetric}");
            sb.AppendLine($"  Parsed: {ev.ParsedMetric}");
            if (ev.ParsedMetric.Labels != null && ev.ParsedMetric.Labels.Any())
            {
                sb.AppendLine("  Labels:");
                foreach (var kv in ev.ParsedMetric.Labels)
                    sb.AppendLine($"    - {kv.Key}: {kv.Value}");
            }

            if (ev.Values != null && ev.Values.Any())
            {
                sb.AppendLine("  Values:");
                foreach (var kv in ev.Values)
                    sb.AppendLine($"    - {kv.Key}: {kv.Value}");
            }

            sb.AppendLine($"  Timestamp: {ev.Timestamp:O}");
            sb.AppendLine($"  State: {ev.State} (was {ev.OldState})");
            sb.AppendLine();
        }

        sb.AppendLine("Contact:");
        if (alert.Contact != null)
        {
            sb.AppendLine($"- Type: {alert.Contact.Type}");
            sb.AppendLine($"- Value: {alert.Contact.Value}");
            sb.AppendLine($"- User: {alert.Contact.User}");
            sb.AppendLine($"- Team: {alert.Contact.Team}");
        }

        sb.AppendLine();
        sb.AppendLine("Provide actionable steps prioritized by safety (do-no-harm first).");
        return sb.ToString();
    }

    private static string ExtractText(JsonElement doc)
    {
        if (doc.ValueKind == JsonValueKind.Object)
        {
            if (doc.TryGetProperty("output", out var o) && o.ValueKind == JsonValueKind.String)
                return o.GetString();
            if (doc.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                return t.GetString();
            // Fallback to serialization
            return doc.ToString();
        }

        return doc.ToString();
    }

    private static List<string>? ExtractActions(string text)
    {
        var actions = new List<string>();
        if (string.IsNullOrEmpty(text))
            return actions;

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("- ") || trimmed.StartsWith("* ")) actions.Add(trimmed.Substring(2).Trim());
        }

        return actions;
    }
}

public class GptOssOptions
{
    public string Endpoint { get; set; }
    public string Model { get; set; }
}