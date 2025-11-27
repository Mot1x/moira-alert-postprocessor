using System.Text;
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
        if (_options.TimeoutSeconds <= 0) _options = new OllamaOptions
        {
            Endpoint = _options.Endpoint,
            Model = _options.Model,
            TimeoutSeconds = 30
        };
        _http.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        Console.WriteLine($"[Startup] NLP provider: Ollama, endpoint={_options.Endpoint}, model={_options.Model}");
    }

    public async Task<Suggestion> GetSuggestionAsync(MoiraAlert alert, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.Model))
        {
            return new Suggestion(
                "NLP не сконфигурирован",
                "Отсутствуют настройки Ollama.Endpoint/Model. Задайте их через appsettings.json, переменные окружения или docker-compose.yml.",
                new List<string>()
            );
        }

        var payload = new
        {
            model = _options.Model,
            prompt = BuildPrompt(alert),
            stream = false
        };

        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsJsonAsync(_options.Endpoint, payload, cancellationToken);
        }
        catch (TaskCanceledException tex)
        {
            return new Suggestion(
                "Таймаут запроса к NLP",
                tex.Message,
                new List<string>()
            );
        }
        catch (HttpRequestException hrex)
        {
            return new Suggestion(
                "Сетевой сбой при обращении к NLP",
                $"{hrex.Message}. Проверьте, запущен ли Ollama на {_options.Endpoint} и доступна ли модель '{_options.Model}'.",
                new List<string>()
            );
        }
        catch (Exception ex)
        {
            return new Suggestion(
                "Неожиданная ошибка при запросе к NLP",
                ex.Message,
                new List<string>()
            );
        }

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            return new Suggestion(
                $"Ошибка запроса к NLP: {(int)resp.StatusCode} {resp.ReasonPhrase}",
                body,
                new List<string>()
            );
        }
        var json = await resp.Content.ReadAsStringAsync(cancellationToken);

        // Попытка извлечения текста от провайдера
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
            // оставить как есть
        }

        // Попытка структурированного JSON-парсинга
        var extracted = OllamaResponseParser.TryParse(text);
        if (extracted != null)
        {
            // steps из JSON
            var steps = extracted.suggested_solution?.steps ?? new List<string>();

            // Fallback: если steps пустой, пробуем извлечь из description
            if (steps.Count == 0 && !string.IsNullOrWhiteSpace(extracted.suggested_solution?.description))
            {
                steps = ExtractStepsFromDescription(extracted.suggested_solution.description);
            }

            var rawCommand = extracted.suggested_solution?.command;
            if (string.IsNullOrWhiteSpace(rawCommand))
            {
                rawCommand = TryExtractCommandFallback(extracted.suggested_solution?.description, steps);
            }
            else
            {
                rawCommand = rawCommand.Trim();
            }
            if (!string.IsNullOrWhiteSpace(rawCommand))
                Console.WriteLine("[NLP] Extracted command: " + rawCommand);

            var summary = extracted.problem_summary ?? "Нет краткого описания";
            var detailsCombined = BuildDetails(extracted);
            return new Suggestion(summary, detailsCombined, steps,
                analysisStatus: extracted.analysis_status,
                isActionable: extracted.is_actionable_by_mcp,
                solutionType: extracted.suggested_solution?.type,
                solutionDescription: extracted.suggested_solution?.description,
                solutionCommand: rawCommand);
        }

        // Fallback на старую логику
        var summaryFallback = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "No summary";
        var detailsFallback = text ?? string.Empty;
        var actionsFallback = ExtractActions(text ?? string.Empty);
        return new Suggestion(summaryFallback, detailsFallback, actionsFallback);
    }

    private static List<string> ExtractStepsFromDescription(string description)
    {
        var steps = new List<string>();
        var raw = description.Replace("\r", " ").Trim();
        if (string.IsNullOrWhiteSpace(raw)) return steps;

        // Пытаемся разбить по номерам "1. ... 2. ..." либо переводам строк
        var matches = System.Text.RegularExpressions.Regex.Matches(raw, @"\b\d+\.\s+.*?(?=(\b\d+\.\s)|$)", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (matches.Count > 0)
        {
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                var txt = System.Text.RegularExpressions.Regex.Replace(m.Value.Trim(), @"^(\d+\.)\s+", string.Empty).Trim();
                if (txt.Length > 0) steps.Add(txt);
            }
        }
        else
        {
            // Разбиваем по строкам или точкам, но осторожно: сначала строки
            var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var t = System.Text.RegularExpressions.Regex.Replace(line.Trim(), @"^(\d+\.)\s+", string.Empty).Trim();
                if (t.Length > 0) steps.Add(t);
            }
            if (steps.Count == 0)
            {
                // Последний fallback: делим по '. ' если описаний несколько
                var parts = raw.Split('.');
                foreach (var p in parts)
                {
                    var t = p.Trim();
                    if (t.Length > 0) steps.Add(t);
                }
            }
        }
        return steps;
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

    private static string BuildDetails(OllamaStructuredSolution data)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(data.problem_summary))
            sb.AppendLine(data.problem_summary.Trim());
        if (!string.IsNullOrWhiteSpace(data.suggested_solution?.description))
            sb.AppendLine(data.suggested_solution.description.Trim());
        return sb.ToString().Trim();
    }

    private string BuildPrompt(MoiraAlert alert)
    {
        var sb = new StringBuilder();
        // Новый строгий JSON-инструктаж
        sb.AppendLine("Ты — ассистент по наблюдаемости. Проанализируй Moira alert и ответь СТРОГО в формате JSON без пояснений, без markdown, без комментариев.");
        sb.AppendLine("Ответь строго в формате JSON. Используй следующую схему:");
        sb.AppendLine("{");
        sb.AppendLine("  \"analysis_status\": \"string (e.g. 'root_cause_identified' | 'needs_more_data')\",");
        sb.AppendLine("  \"problem_summary\": \"string (1-2 предложения краткого корневого анализа)\",");
        sb.AppendLine("  \"suggested_solution\": {");
        sb.AppendLine("    \"type\": \"string (e.g. 'config_change' | 'scale_out' | 'investigate')\",");
        sb.AppendLine("    \"steps\": [ \"строка шага без нумерации\", \"ещё один шаг\" ],");
        sb.AppendLine("    \"command\": \"string (одна безопасная команда или последовательность; если нет — пустая строка)\"");
        sb.AppendLine("  },");
        sb.AppendLine("  \"is_actionable_by_mcp\": true,");
        sb.AppendLine("  \"confidence\": 0.0");
        sb.AppendLine("}");
        sb.AppendLine("Только JSON. Никакого текста вне {}. 'steps' обязательный массив (может быть пустым). Каждый элемент steps — одна законченная инструкция без ведущих '1.' или '-' и без лишних пробелов. НЕ используй markdown.");
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

    private static string? TryExtractCommandFallback(string? description, List<string> steps)
    {
        // Ищем первую строку, похожую на команду (начинается с curl, kubectl, docker, systemctl, ping, tail, grep, cat)
        IEnumerable<string> sources = Enumerable.Empty<string>();
        if (!string.IsNullOrWhiteSpace(description))
            sources = sources.Append(description);
        if (steps != null && steps.Count > 0)
            sources = sources.Concat(steps);

        foreach (var s in sources)
        {
            var lines = s.Split('\n');
            foreach (var line in lines)
            {
                var candidate = line.Trim();
                if (candidate.Length == 0) continue;
                if (StartsWithCommand(candidate)) return candidate;
            }
        }
        return null;

        static bool StartsWithCommand(string s) =>
            s.StartsWith("curl ", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("kubectl ", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("docker ", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("systemctl ", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("ping ", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("grep ", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("cat ", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("tail ", StringComparison.OrdinalIgnoreCase);
    }
}
