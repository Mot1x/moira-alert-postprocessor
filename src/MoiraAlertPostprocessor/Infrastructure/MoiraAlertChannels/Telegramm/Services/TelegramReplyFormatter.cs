using System.Linq;
using System.Text;
using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Services;

public class TelegramReplyFormatter : ITelegramReplyFormatter
{
    public string Format(Suggestion suggestion)
    {
        var sb = new StringBuilder();

        // Статус анализа
        if (!string.IsNullOrWhiteSpace(suggestion.AnalysisStatus))
            sb.AppendLine($"<b>Статус анализа:</b> {Escape(suggestion.AnalysisStatus)}");

        // Проблема
        if (!string.IsNullOrWhiteSpace(suggestion.Summary))
            sb.AppendLine($"<b>Проблема:</b> {Escape(suggestion.Summary)}");
        else if (!string.IsNullOrWhiteSpace(suggestion.Details))
            sb.AppendLine($"<b>Проблема:</b> {Escape(ExtractProblem(suggestion.Details))}");

        // Тип решения (берём напрямую из SolutionType)
        if (!string.IsNullOrWhiteSpace(suggestion.SolutionType))
            sb.AppendLine($"<b>Тип решения:</b> {Escape(suggestion.SolutionType)}");

        // Решение: нумерованные шаги из SolutionDescription (разбиваем по точке/переводу строки) или Actions
        var steps = suggestion.Actions.ToList();
        if (steps.Count > 0)
        {
            sb.AppendLine("<b>Решение:</b>");
            sb.AppendLine("<pre>");
            for (int i = 0; i < steps.Count; i++)
                sb.AppendLine($"    {i + 1}. {Escape(steps[i])}");
            sb.AppendLine("</pre>");
        }

        // Команда
        if (!string.IsNullOrWhiteSpace(suggestion.SolutionCommand))
        {
            sb.AppendLine("<b>Команда:</b>");
            sb.AppendLine("<pre>" + EscapeCode(suggestion.SolutionCommand) + "</pre>");
        }

        // Автоматизируемо
        if (suggestion.IsActionableByMcp.HasValue)
            sb.AppendLine($"<b>Автоматизируемо:</b> {(suggestion.IsActionableByMcp.Value ? "Да" : "Нет")}");

        return sb.ToString().TrimEnd();
    }

    private static List<string> ExtractSteps(Suggestion suggestion)
    {
        // Упразднено: теперь steps попадают напрямую в Suggestion.Actions из массива suggested_solution.steps.
        return suggestion.Actions.ToList();
    }

    static string Escape(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    static string EscapeCode(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("`", "&#96;");

    static string ExtractProblem(string details)
    {
        // Пытаемся извлечь первую смысловую строку как описание проблемы.
        // Если в тексте есть маркер '*Проблема:*', забираем его содержимое.
        var idx = details.IndexOf("Проблема:", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var after = details.Substring(idx + "Проблема:".Length).Trim().TrimStart('*', ':');
            var endLine = after.IndexOf('\n');
            return endLine >= 0 ? after.Substring(0, endLine).Trim() : after.Trim();
        }
        // Иначе берём первую строку/абзац
        var firstLineEnd = details.IndexOf('\n');
        return firstLineEnd >= 0 ? details.Substring(0, firstLineEnd).Trim() : details.Trim();
    }

    static string NormalizeDetailsForSolution(string details) => details;
    static string RemoveLinesStartingWith(string text, string prefix) => text;
    static string? InferSolutionType(string? analysisStatus, IEnumerable<string>? actions) => analysisStatus;
}
