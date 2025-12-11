using MoiraAlertPostprocessor.Core.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Interfaces;

/// <summary>
/// Форматирует Suggestion в текстовое сообщение, пригодное для отправки в Telegram.
/// </summary>
public interface ITelegramReplyFormatter
{
    string Format(Suggestion suggestion);
}
