using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoiraAlertPostprocessor.Infrastructure.NlServices;
using MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Interfaces;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm;

public class TelegramUpdateWorker : BackgroundService
{
    private readonly ILogger<TelegramUpdateWorker> _logger;
    private readonly TelegramBotClient _botClient;
    private readonly IMoiraAlertChannel _channel;
    private readonly ITelegramPostParser _postParser;
    private readonly ITelegramReplyFormatter _replyFormatter;
    private readonly INlpService _nlpService;

    public TelegramUpdateWorker(
        IOptions<TelegramAlertOptions> options,
        IMoiraAlertChannel channel,
        ITelegramPostParser postParser,
        ITelegramReplyFormatter replyFormatter,
        INlpService nlpService,
        ILogger<TelegramUpdateWorker> logger)
    {
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(opts.BotToken))
            throw new ArgumentException("BotToken не может быть пустым", nameof(options));

        _logger = logger;
        _botClient = new TelegramBotClient(opts.BotToken);
        _channel = channel;
        _postParser = postParser;
        _replyFormatter = replyFormatter;
        _nlpService = nlpService;
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
            if (update.Type == UpdateType.Message && update.Message is { } msg)
            {
                if (!_postParser.TryParse(msg, out var alert))
                    return;

                var suggestion = await _nlpService.GetSuggestionAsync(alert!, ct);
                var reply = _replyFormatter.Format(suggestion);

                await _channel.SendReplyToMessageAsync(msg.Chat.Id, msg.MessageId, reply, ct);
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