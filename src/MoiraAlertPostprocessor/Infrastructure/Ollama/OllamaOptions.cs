using System;

namespace MoiraAlertPostprocessor.Infrastructure.Ollama;

public class OllamaOptions
{
    public string Endpoint { get; init; }
    public string Model { get; init; }
    public int TimeoutSeconds { get; init; }
}