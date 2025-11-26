using System.Linq;
using System.Text;
using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Services;

public class TelegramReplyFormatter : ITelegramReplyFormatter
{
    public string Format(Suggestion suggestion)
    {
        // Попробуем понять, находится ли Details уже в Telegram-markup.
        // Предположим, что Suggestion.Details сформировано OllamaResponseParser.ToTelegramMarkup.
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(suggestion.Summary))
        {
            sb.AppendLine($"<b>Сводка:</b> {Escape(suggestion.Summary)}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(suggestion.Details))
        {
            sb.AppendLine(suggestion.Details);
            sb.AppendLine();
        }

        if (suggestion.Actions != null && suggestion.Actions.Any())
        {
            sb.AppendLine("<b>Действия:</b>");
            sb.AppendLine("<pre>");
            foreach (var act in suggestion.Actions)
                sb.AppendLine(Escape(act));
            sb.AppendLine("</pre>");
        }

        if (!string.IsNullOrWhiteSpace(suggestion.AnalysisStatus))
            sb.AppendLine($"<i>Статус анализа: {Escape(suggestion.AnalysisStatus)}</i>");

        return sb.ToString().TrimEnd();

        static string Escape(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}
