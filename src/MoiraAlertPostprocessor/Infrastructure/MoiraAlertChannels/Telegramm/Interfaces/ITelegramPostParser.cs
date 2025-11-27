namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Interfaces;

using MoiraAlertPostprocessor.Domain.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

/// <summary>
/// Абстракция парсера Telegram-поста, возвращающего MoiraAlert.
/// Если пост не является алертом Moira — метод возвращает false.
/// </summary>
public interface ITelegramPostParser
{
    bool TryParse(ITelegramBotClient botClient, Message post, out MoiraAlert? alert);
}
