namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm;

using MoiraAlertPostprocessor.Domain.Entities;

/// <summary>
/// Форматирует Suggestion в текстовое сообщение, пригодное для отправки в Telegram.
/// </summary>
public interface ITelegramReplyFormatter
{
    string Format(Suggestion suggestion);
}
