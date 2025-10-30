using System;

namespace MoiraAlertPostprocessor.Infrastructure.Ollama;

public class OllamaOptions
{
    public string Endpoint { get; set; } = "http://localhost:11434/api/generate";
    public string Model { get; set; } = "deepseek-v3.1:671b-cloud";
    public int TimeoutSeconds { get; set; } = 30;
}

