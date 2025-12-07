using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Core.Domain.Entities.Mcp;

public record McpRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = default!;

    [JsonPropertyName("data")]
    public object? Data { get; init; }
}