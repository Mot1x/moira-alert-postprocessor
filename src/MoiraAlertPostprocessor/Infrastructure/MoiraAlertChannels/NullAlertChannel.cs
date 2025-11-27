using System.Text.Json;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels;

/// <summary>
/// Пустой канал оповещений: логирует сообщение и ничего никуда не отправляет.
/// Нужен для локального теста без Telegram.
/// </summary>
public class NullAlertChannel : IMoiraAlertChannel
{
    private readonly ILogger<NullAlertChannel> _logger;
    public NullAlertChannel(ILogger<NullAlertChannel> logger)
    {
        _logger = logger;
    }

    public async Task AlertUsersAsync(JsonContent msg, CancellationToken cancellationToken = default)
    {
        try
        {
            using var stream = msg.ReadAsStream();
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var jsonText = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("[NullAlertChannel] would send alert:\n{Json}", jsonText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NullAlertChannel] failed to log alert json");
        }
    }

    public Task SendReplyToPostAsync(int channelPostMessageId, string text, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NullAlertChannel] would reply to post {Id}: {Text}", channelPostMessageId, text);
        return Task.CompletedTask;
    }

    public Task SendFeedbackButtonsAsync(long chatId, int replyToMessageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NullAlertChannel] would reply to message {ChatId} - {MessageId} with feedback vote", chatId, replyToMessageId);
        return Task.CompletedTask;
    }

    public Task SendReplyToMessageAsync(long chatId, int replyToMessageId, string text,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NullAlertChannel] would reply in chat {ChatId} to message {MessageId}: {Text}",
            chatId, replyToMessageId, text);
        return Task.CompletedTask;
    }
}

