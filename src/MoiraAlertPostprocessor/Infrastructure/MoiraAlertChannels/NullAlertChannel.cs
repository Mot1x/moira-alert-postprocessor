using System.Text.Json;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels;

/// <summary>
///     Пустой канал оповещений: логирует сообщение и ничего никуда не отправляет.
///     Нужен для локального теста без Telegram.
/// </summary>
public class NullAlertChannel(ILogger<NullAlertChannel> logger) : IMoiraAlertChannel
{
    public async Task AlertUsersAsync(JsonContent msg, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = await msg.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var jsonText = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            logger.LogInformation("[NullAlertChannel] would send alert:\n{Json}", jsonText);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[NullAlertChannel] failed to log alert json");
        }
    }

    public Task SendReplyToPostAsync(int channelPostMessageId, string text,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[NullAlertChannel] would reply to post {Id}: {Text}", channelPostMessageId, text);
        return Task.CompletedTask;
    }

    public Task SendFeedbackButtonsAsync(long chatId, int replyToMessageId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[NullAlertChannel] would reply to message {ChatId} - {MessageId} with feedback vote",
            chatId, replyToMessageId);
        return Task.CompletedTask;
    }

    public Task SendReplyToMessageAsync(long chatId, int replyToMessageId, string text,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[NullAlertChannel] would reply in chat {ChatId} to message {MessageId}: {Text}",
            chatId, replyToMessageId, text);
        return Task.CompletedTask;
    }
}