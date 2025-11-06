using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;

public class OllamaClient : INlpService
{
    private readonly HttpClient _http;
    private readonly OllamaOptions _options;

    public OllamaClient(HttpClient http, IOptions<OllamaOptions> options)
    {
        _http = http;
        _options = options.Value;
        _http.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        Console.WriteLine($"[Startup] NLP provider: Ollama, endpoint={_options.Endpoint}, model={_options.Model}");
    }

    public async Task<Suggestion> GetSuggestionAsync(MoiraAlert alert, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = _options.Model,
            prompt = BuildPrompt(alert),
            stream = false
        };

        using var resp = await _http.PostAsJsonAsync(_options.Endpoint, payload, cancellationToken);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(cancellationToken);

        // Try parse common shapes
        string text = json;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("response", out var rr) && rr.ValueKind == JsonValueKind.String)
                    text = rr.GetString() ?? json;
                else if (doc.RootElement.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                    text = t.GetString() ?? json;
                else if (doc.RootElement.TryGetProperty("result", out var r) && r.ValueKind == JsonValueKind.String)
                    text = r.GetString() ?? json;
                else if (doc.RootElement.TryGetProperty("output", out var o) && o.ValueKind == JsonValueKind.String)
                    text = o.GetString() ?? json;
                else
                    text = doc.RootElement.ToString();
            }
            else
            {
                text = doc.RootElement.ToString();
            }
        }
        catch
        {
            // leave raw json
        }

        var summary = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "No summary";
        var details = text ?? string.Empty;
        var actions = ExtractActions(text ?? string.Empty);

        return new Suggestion(summary, details, actions);
    }

    private string BuildPrompt(MoiraAlert alert)
    {
        var sb = new System.Text.StringBuilder();
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
        sb.AppendLine("Write in Russian.");
        return sb.ToString();
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
