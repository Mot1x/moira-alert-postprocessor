using Microsoft.Extensions.Options;
using MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm;

public class TelegramAlertChannel : IMoiraAlertChannel
{
    private readonly TelegramBotClient _botClient;
    private readonly string _channelChatId;
    private readonly string? _discussionGroupChatId;
    private readonly ILogger<TelegramAlertChannel> _logger;

    public TelegramAlertChannel(
        IOptions<TelegramAlertOptions> options,
        ILogger<TelegramAlertChannel> logger)
    {
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(opts.BotToken))
            throw new ArgumentException("BotToken не может быть пустым", nameof(options));
        if (string.IsNullOrWhiteSpace(opts.ChatId))
            throw new ArgumentException("ChatId не может быть пустым", nameof(options));

        _botClient = new TelegramBotClient(opts.BotToken);
        _channelChatId = opts.ChatId;
        _discussionGroupChatId = opts.DiscussionGroupChatId;
        _logger = logger;
    }

    public async Task AlertUsersAsync(JsonContent msg, CancellationToken cancellationToken = default)
    {
        try
        {
            using var stream = msg.ReadAsStream();
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var jsonText = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            await SendAlertAsync(jsonText, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Отправка оповещения отменена.");
            throw;
        }
        catch (ApiRequestException ex)
        {
            _logger.LogError(ex, "Ошибка Telegram API ({ErrorCode}): {Message}", ex.ErrorCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при отправке оповещения в Telegram");
            throw;
        }
    }

    public async Task SendReplyToPostAsync(int channelPostMessageId, string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_discussionGroupChatId))
            throw new InvalidOperationException("DiscussionGroupChatId не задан в настройках, отправка ответа под постом невозможна.");

        const int MaxMessageLength = 4096;
        var message = text.Length <= MaxMessageLength ? text : text[..MaxMessageLength] + "\n\n[... обрезано]";

        await _botClient.SendMessage(
            chatId: new ChatId(_discussionGroupChatId),
            text: message,
            messageThreadId: channelPostMessageId,
            parseMode: ParseMode.Html,
            linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            cancellationToken: ct
        );
    }

    private async Task SendAlertAsync(string jsonText, CancellationToken ct)
    {
        const int MaxMessageLength = 4096;
        var message = jsonText.Length <= MaxMessageLength ? jsonText : jsonText[..MaxMessageLength] + "\n\n[... обрезано]";
        var escaped = EscapeHtml(message);
        var formatted = $"<pre>{escaped}</pre>";

        await _botClient.SendMessage(
            chatId: new ChatId(_channelChatId),
            text: formatted,
            parseMode: ParseMode.Html,
            linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            cancellationToken: ct
        );
    }

    private static string EscapeHtml(string s) =>
        s.Replace("&", "&amp;")
         .Replace("<", "&lt;")
         .Replace(">", "&gt;");
}