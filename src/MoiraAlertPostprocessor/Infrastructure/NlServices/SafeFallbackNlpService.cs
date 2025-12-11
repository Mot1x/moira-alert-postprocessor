using MoiraAlertPostprocessor.Core.Domain.Entities;
using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices;

/// <summary>
///     Безопасная заглушка для NLP. Возвращает статичную подсказку, если Ollama не настроен.
/// </summary>
public class SafeFallbackNlpService : INlpService
{
    public Task<Suggestion> GetSuggestionAsync(MoiraAlert alert, CancellationToken cancellationToken = default)
    {
        var summary = "НЛП не настроен: укажите Ollama.Endpoint и Ollama.Model в конфигурации.";
        var details =
            "Сервис запущен в режиме заглушки. Подключите Ollama или задайте переменные окружения Ollama__Endpoint и Ollama__Model.";
        return Task.FromResult(new Suggestion(summary, details, new List<string>()));
    }
}