namespace MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;

public class TelegramAlertOptions
{
    public const string SectionName = "TelegramAlert";

    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string? DiscussionGroupChatId { get; set; }
}