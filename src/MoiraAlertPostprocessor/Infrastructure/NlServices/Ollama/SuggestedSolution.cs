using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;

public record SuggestedSolution(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("command")]
    string? Command,
    [property: JsonPropertyName("mcp_server")]
    string? McpServer,
    [property: JsonPropertyName("tool_name")]
    string? ToolName,
    [property: JsonPropertyName("tool_args")]
    Dictionary<string, string>? ToolArgs);