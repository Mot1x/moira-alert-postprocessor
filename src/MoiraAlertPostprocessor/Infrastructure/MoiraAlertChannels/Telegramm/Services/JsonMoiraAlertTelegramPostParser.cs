using System.Text.Json;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MoiraAlertPostprocessor.API.Models.Request;
using MoiraAlertPostprocessor.Domain.Entities;
using Telegram.Bot.Types;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Interfaces;

namespace MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Services;

/// <summary>
/// Простой парсер: пытается распарсить текст поста как JSON Moira webhook.
/// </summary>
public class JsonMoiraAlertTelegramPostParser : ITelegramPostParser
{
    private readonly IMapper _mapper;
    private readonly ILogger<JsonMoiraAlertTelegramPostParser> _logger;

    public JsonMoiraAlertTelegramPostParser(IMapper mapper, ILogger<JsonMoiraAlertTelegramPostParser> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public bool TryParse(Message post, out MoiraAlert? alert)
    {
        alert = null;
        var text = post.Text ?? post.Caption;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();
        if (!text.StartsWith("{") || !text.EndsWith("}"))
            return false;

        try
        {
            var dto = JsonSerializer.Deserialize<IncomingMoiraWebhookDto>(text);
            if (dto == null)
                return false;
            alert = _mapper.Map<MoiraAlert>(dto);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Не удалось распарсить пост как Moira JSON");
            return false;
        }
    }
}
