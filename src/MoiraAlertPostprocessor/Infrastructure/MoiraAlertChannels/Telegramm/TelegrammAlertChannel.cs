using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm;

public class TelegramAlertChannel : IMoiraAlertChannel
{
    private readonly TelegramBotClient _botClient;
    private readonly string _chatId;
    private readonly ILogger<TelegramAlertChannel> _logger;

    public TelegramAlertChannel(
        IOptions<TelegramAlertOptions> options,
        ILogger<TelegramAlertChannel> logger)
    {
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(opts.BotToken))
            throw new ArgumentException("BotToken не может быть пустым", nameof(opts.BotToken));
        if (string.IsNullOrWhiteSpace(opts.ChatId))
            throw new ArgumentException("ChatId не может быть пустым", nameof(opts.ChatId));

        _botClient = new TelegramBotClient(opts.BotToken);
        _chatId = opts.ChatId;
        _logger = logger;
    }

    public async Task AlertUsersAsync(JsonContent msg, CancellationToken cancellationToken = default)
    {
        try
        {
            // Преобразуем JsonContent → string (с pretty-print)
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
            _logger.LogError(ex,
                "Ошибка Telegram API ({ErrorCode}): {Message}",
                ex.ErrorCode, ex.Message);
            throw; // или swallow, если нужно
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при отправке оповещения в Telegram");
            throw;
        }
    }

    private async Task SendAlertAsync(string jsonText, CancellationToken ct)
    {
        const int MaxMessageLength = 4096;
        var message = jsonText.Length <= MaxMessageLength
            ? jsonText
            : jsonText[..MaxMessageLength] + "\n\n[... обрезано]";

        // Используем <pre> + HTML-экранирование для читаемости
        var escaped = EscapeHtml(message);
        var formatted = $"<pre>{escaped}</pre>";

        await _botClient.SendTextMessageAsync(
            chatId: _chatId,
            text: formatted,
            parseMode: ParseMode.Html,
            disableWebPagePreview: true,
            cancellationToken: ct
        );
    }

    private static string EscapeHtml(string s) =>
        s.Replace("&", "&amp;")
         .Replace("<", "<")
         .Replace(">", ">");
}