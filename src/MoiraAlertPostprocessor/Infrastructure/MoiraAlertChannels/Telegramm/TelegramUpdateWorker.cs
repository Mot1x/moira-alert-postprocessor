using Microsoft.Extensions.Options;
using MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm;

public class TelegramUpdateWorker : BackgroundService
{
    private readonly ILogger<TelegramUpdateWorker> _logger;
    private readonly TelegramBotClient _botClient;
    private readonly IMoiraAlertChannel _channel;

    public TelegramUpdateWorker(
        IOptions<TelegramAlertOptions> options,
        IMoiraAlertChannel channel,
        ILogger<TelegramUpdateWorker> logger)
    {
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(opts.BotToken))
            throw new ArgumentException("BotToken не может быть пустым", nameof(options));

        _logger = logger;
        _botClient = new TelegramBotClient(opts.BotToken);
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            updateHandler: new DefaultUpdateHandler(HandleUpdateAsync, HandlePollingErrorAsync),
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("TelegramUpdateWorker запущен");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update.Type == UpdateType.ChannelPost && update.ChannelPost is { } post)
            {
                await _channel.SendReplyToPostAsync(post.MessageId, "Hello World!", ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке апдейта Telegram");
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        var errorMsg = exception switch
        {
            ApiRequestException apiEx => $"Telegram API Error: [{apiEx.ErrorCode}] {apiEx.Message}",
            _ => exception.Message
        };
        _logger.LogError("Polling error: {Message}", errorMsg);
        return Task.CompletedTask;
    }
}