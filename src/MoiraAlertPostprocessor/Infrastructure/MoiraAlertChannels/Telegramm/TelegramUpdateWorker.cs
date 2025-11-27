using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoiraAlertPostprocessor.Infrastructure.NlServices;
using MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;
using MoiraAlertPostprocessor.Infrastructure.Services;
using MoiraAlertPostprocessor.Infrastructure.Repositories;
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
    private readonly IVoteRepository _voteRepository;
    private readonly FeedbackMetricsService _metricsService;

    public TelegramUpdateWorker(
        IOptions<TelegramAlertOptions> options,
        IMoiraAlertChannel channel,
        ITelegramPostParser postParser,
        ITelegramReplyFormatter replyFormatter,
        INlpService nlpService,
        IVoteRepository voteRepository,
        FeedbackMetricsService metricsService,
        ILogger<TelegramUpdateWorker> logger)
    {
        var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(opts.BotToken))
            throw new ArgumentException("BotToken не может быть пустым", nameof(options));

        _logger = logger;
        _botClient = new TelegramBotClient(opts.BotToken);
        
        if (channel is TelegramAlertChannel tgChannel)
            _channel = tgChannel;
        else
            throw new InvalidOperationException("TelegramUpdateWorker requires TelegramAlertChannel");

        _postParser = postParser;
        _replyFormatter = replyFormatter;
        _nlpService = nlpService;
        _voteRepository = voteRepository;
        _metricsService = metricsService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
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
                await HandleMessageAsync(msg, ct);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is { } callback)
            {
                await HandleCallbackAsync(callback, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Telegram update");
        }
    }

    private async Task HandleMessageAsync(Message msg, CancellationToken ct)
    {
        if (!_postParser.TryParse(_botClient, msg, out var alert))
            return;

        var suggestion = await _nlpService.GetSuggestionAsync(alert!, ct);
        var reply = _replyFormatter.Format(suggestion);
        if (msg.Chat.Type == ChatType.Channel)
        {
            await _channel.SendReplyToPostAsync(msg.MessageId, reply, ct);
        }
        else
        {
            await _channel.SendReplyToMessageAsync(msg.Chat.Id, msg.MessageId, reply, ct);
            var botReplyId = msg.MessageId + 1; 
            await _channel.SendFeedbackButtonsAsync(msg.Chat.Id, msg.MessageId, ct);
        }

    }

    private async Task HandleCallbackAsync(CallbackQuery callback, CancellationToken ct)
    {
        var data = callback.Data;
        var messageId = callback.Message?.MessageId ?? 0;
        var chatId = callback.Message?.Chat.Id ?? 0;
        var userId = callback.From.Id;

        if (string.IsNullOrEmpty(data) || !data.StartsWith("vote_"))
            return;

        if (!_voteRepository.TryVote(chatId, messageId, userId))
        {
            await _botClient.AnswerCallbackQuery(
                callbackQueryId: callback.Id,
                text: "Вы уже голосовали за этот ответ.",
                showAlert: false,
                cancellationToken: ct);
            return;
        }

        var rating = data == "vote_like" ? "like" : "dislike";
        _metricsService.RecordFeedback(rating);

        await _botClient.AnswerCallbackQuery(
            callbackQueryId: callback.Id,
            text: "Спасибо за отзыв!",
            showAlert: false,
            cancellationToken: ct);
            
        try 
        {
             await _botClient.EditMessageText(
                chatId: new ChatId(chatId),
                messageId: messageId,
                text: $"Спасибо! Ваш голос учтен ({rating}).",
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке feedback Telegram");
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