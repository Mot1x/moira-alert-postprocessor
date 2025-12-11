using System.Text.Json;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;

public record SuggestedSolution(string? type, string? description, string? command, List<string>? steps = null);

public record OllamaStructuredSolution(
    string? analysis_status,
    string? problem_summary,
    SuggestedSolution? suggested_solution,
    bool? is_actionable_by_mcp,
    double? confidence
);

internal static class OllamaResponseParser
{
    public static OllamaStructuredSolution? TryParse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var jsonSpan = ExtractFirstJsonObject(raw);
        if (jsonSpan == null) return null;

        try
        {
            using var doc = JsonDocument.Parse(jsonSpan);
            var root = doc.RootElement;
            var analysis = root.TryGetProperty("analysis_status", out var a) ? a.GetString() : null;
            var summary = root.TryGetProperty("problem_summary", out var ps) ? ps.GetString() : null;

            SuggestedSolution? sol = null;
            if (root.TryGetProperty("suggested_solution", out var ss) && ss.ValueKind == JsonValueKind.Object)
                sol = new SuggestedSolution(
                    ss.TryGetProperty("type", out var t) ? t.GetString() : null,
                    ss.TryGetProperty("description", out var d) ? d.GetString() : null,
                    ss.TryGetProperty("command", out var c) ? c.GetString() : null,
                    ss.TryGetProperty("steps", out var st) && st.ValueKind == JsonValueKind.Array
                        ? st.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String)
                            .Select(e => e.GetString()!).Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                        : null
                );

            bool? actionable = root.TryGetProperty("is_actionable_by_mcp", out var ia) &&
                               ia.ValueKind == JsonValueKind.True
                ? true
                : root.TryGetProperty("is_actionable_by_mcp", out ia) && ia.ValueKind == JsonValueKind.False
                    ? false
                    : null;

            double? confidence = null;
            if (root.TryGetProperty("confidence", out var conf) && conf.ValueKind is JsonValueKind.Number)
                if (conf.TryGetDouble(out var dd))
                    confidence = dd;

            return new OllamaStructuredSolution(analysis, summary, sol, actionable, confidence);
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractFirstJsonObject(string raw)
    {
        var start = raw.IndexOf('{');
        if (start < 0) return null;
        var braceDepth = 0;
        for (var i = start; i < raw.Length; i++)
        {
            var ch = raw[i];
            if (ch == '{') braceDepth++;
            if (ch == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                {
                    var candidate = raw.Substring(start, i - start + 1).Trim();
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("&", "&amp;");
    }

    private static string EscapeCode(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        // Для моноширинного блока достаточно экранировать обратные кавычки
        return s.Replace("`", "\\`");
    }
}