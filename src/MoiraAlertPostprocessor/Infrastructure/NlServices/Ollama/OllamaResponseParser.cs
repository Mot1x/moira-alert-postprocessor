using System;
using System.Text;
using System.Text.Json;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;

internal static class OllamaResponseParser
{
    public static OllamaStructuredSolution? TryParse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var jsonSpan = ExtractFirstJsonObject(raw);
        if (jsonSpan == null) return null;

        try
        {
            return JsonSerializer.Deserialize<OllamaStructuredSolution>(jsonSpan, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
        }
        catch
        {
            return null;
        }
    }

    public static string ToTelegramMarkup(OllamaStructuredSolution data)
    {
        var sb = new StringBuilder();

        void Bold(string title, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                sb.AppendLine($"*{Escape(title)}* {Escape(value)}");
        }

        Bold("Анализ:", data.AnalysisStatus);
        Bold("Проблема:", data.ProblemSummary);

        if (data.SuggestedSolution != null)
        {
            Bold("Тип решения:", data.SuggestedSolution.Type);
            if (!string.IsNullOrWhiteSpace(data.SuggestedSolution.Description))
                sb.AppendLine($"*Решение:* {Escape(data.SuggestedSolution.Description)}");
            if (!string.IsNullOrWhiteSpace(data.SuggestedSolution.Command))
            {
                sb.AppendLine("*Команда:*");
                sb.AppendLine("```");
                sb.AppendLine(EscapeCode(data.SuggestedSolution.Command));
                sb.AppendLine("```");
            }
        }

        if (data.IsActionableByMcp.HasValue)
            Bold("Автоматизируемо:", data.IsActionableByMcp.Value ? "Да" : "Нет");

        return sb.ToString().TrimEnd();
    }

    private static string? ExtractFirstJsonObject(string raw)
    {
        int start = raw.IndexOf('{');
        if (start < 0) return null;
        int braceDepth = 0;
        for (int i = start; i < raw.Length; i++)
        {
            char ch = raw[i];
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