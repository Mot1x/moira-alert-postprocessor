using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Core.Domain.Entities.Mcp;

public record McpResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; init; }

    [JsonPropertyName("result")]
    public object? Result { get; init; }

    [JsonPropertyName("error")]
    public McpRpcError? Error { get; init; }
}