using System.Text.Json;
using Microsoft.Extensions.Logging;
using AutoMapper;
using MoiraAlertPostprocessor.API.Models.Request;
using MoiraAlertPostprocessor.Domain.Entities;
using Telegram.Bot.Types;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Interfaces;
using Telegram.Bot;

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

    public bool TryParse(ITelegramBotClient botClient, Message post, out MoiraAlert? alert)
    {
        alert = null;

        // Если документ .json прикреплен
        if (post.Document is { } doc && doc.FileName != null && doc.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var file = botClient.GetFile(doc.FileId).GetAwaiter().GetResult();
                using var ms = new MemoryStream();
                botClient.DownloadFile(file.FilePath!, ms).GetAwaiter().GetResult();
                ms.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(ms);
                var json = reader.ReadToEnd();
                var dto = JsonSerializer.Deserialize<IncomingMoiraWebhookDto>(json);
                if (dto == null)
                    return false;
                alert = _mapper.Map<MoiraAlert>(dto);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Не удалось распарсить документ .json как Moira JSON");
                return false;
            }
        }

        // Фолбэк: пробуем текст/подпись как JSON
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
