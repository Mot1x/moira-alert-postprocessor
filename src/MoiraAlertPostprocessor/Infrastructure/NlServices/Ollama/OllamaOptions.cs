using System;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;

/// <summary>
/// Настройки клиента Ollama: endpoint, модель и таймаут.
/// </summary>
public class OllamaOptions
{
    public string Endpoint { get; init; }
    public string Model { get; init; }
    public int TimeoutSeconds { get; init; }
}