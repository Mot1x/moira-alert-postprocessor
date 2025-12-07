using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Core.Domain.Entities.Mcp;

public record McpRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; init; } // может быть строкой, числом или null (notification)

    [JsonPropertyName("method")]
    public string Method { get; init; } = default!;

    [JsonPropertyName("params")]
    public JsonElement Params { get; init; }
}