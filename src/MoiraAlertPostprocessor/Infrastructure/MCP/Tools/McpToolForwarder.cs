using System.Text;
using System.Text.Json;

namespace MoiraAlertPostprocessor.Infrastructure.MCP.Tools;

public class McpToolForwarder : IMcpTool
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _mcpServersUri = new()
    {
        // Пример:
        // { "k8s-mcp", "http://k8s-mcp:8080/mcp" },
        // { "git-mcp", "http://git-mcp:9000/mcp" }
    };

    public string Name => "forward_tool";

    public string Description => 
        "Перенаправляет вызов инструмента на указанный MCP-сервер.\n" +
        "Аргументы:\n" +
        "- mcp_server_name: имя сервера (из списка)\n" +
        "- tool_name: имя инструмента на удалённом сервере\n" +
        "- arguments: JSON-объект с аргументами (в виде строки)";

    public McpToolForwarder(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ToolExecutionResult> ExecuteAsync(Dictionary<string, string> arguments, CancellationToken ct = default)
    {
        try
        {
            if (!arguments.TryGetValue("mcp_server_name", out var serverName) || string.IsNullOrWhiteSpace(serverName))
                throw new ArgumentException("Требуется 'mcp_server_name'");
            
            if (!_mcpServersUri.TryGetValue(serverName, out var serverUri))
                throw new ArgumentException($"Неизвестный MCP-сервер: {serverName}");

            if (!arguments.TryGetValue("tool_name", out var toolName) || string.IsNullOrWhiteSpace(toolName))
                throw new ArgumentException("Требуется 'tool_name'");

            if (!arguments.TryGetValue("arguments", out var argsJson) || string.IsNullOrWhiteSpace(argsJson))
                throw new ArgumentException("Требуется 'arguments' (должен быть JSON-объектом в виде строки)");
            
            var toolArgs = JsonSerializer.Deserialize<Dictionary<string, string>>(argsJson) 
                           ?? throw new ArgumentException("arguments должен быть валидным JSON-объектом");
            
            var rpcRequest = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString("N"),
                method = "call",
                @params = new
                {
                    name = toolName,
                    arguments = toolArgs
                }
            };

            var json = JsonSerializer.Serialize(rpcRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(serverUri, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                return new ToolExecutionResult(false, Error: $"HTTP {response.StatusCode}: {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("error", out var errorElem))
            {
                var message = errorElem.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                return new ToolExecutionResult(false, Error: $"MCP error: {message}");
            }
            
            if (root.TryGetProperty("result", out var resultElem))
            {
                string output = resultElem.ValueKind switch
                {
                    JsonValueKind.String => resultElem.GetString() ?? "",
                    _ => resultElem.GetRawText()
                };
                return new ToolExecutionResult(true, Output: output);
            }

            return new ToolExecutionResult(true, Output: responseBody);
        }
        catch (JsonException jex)
        {
            return new ToolExecutionResult(false, Error: $"Ошибка парсинга JSON в 'arguments': {jex.Message}");
        }
        catch (Exception ex)
        {
            return new ToolExecutionResult(false, Error: $"Ошибка перенаправления: {ex.Message}");
        }
    }
}