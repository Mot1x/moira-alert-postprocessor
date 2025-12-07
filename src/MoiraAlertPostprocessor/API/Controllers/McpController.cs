using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MoiraAlertPostprocessor.Core.Domain.Entities.Mcp;
using MoiraAlertPostprocessor.Infrastructure.MCP;

namespace MoiraAlertPostprocessor.API.Controllers;
//этот контроллер на случай, если в будущем будет полноценная поддержка mcp, т е нейронка будет обращаться к нам сама,
//а не через ответ из промта вызывать тулы
//но вообще это в отдельный сервис так то, навверное, надо
[ApiController]
[Route("/mcp")]
public class McpController : ControllerBase
{
    private readonly IEnumerable<IMcpTool> _tools;
    private readonly ILogger<McpController> _logger;

    public McpController(IEnumerable<IMcpTool> tools, ILogger<McpController> logger)
    {
        _tools = tools;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleRpcRequest([FromBody] JsonElement requestJson, CancellationToken ct)
    {
        try
        {
            if (requestJson.ValueKind == JsonValueKind.Array)
            {
                var results = new List<object>();
                foreach (var item in requestJson.EnumerateArray())
                {
                    var response = await ProcessSingleRequestAsync(item, ct);
                    if (response.Id != null) // notifications (id=null) не возвращаются
                        results.Add(response);
                }

                return results.Count == 0 ? NoContent() : Ok(results);
            }
            else
            {
                var response = await ProcessSingleRequestAsync(requestJson, ct);
                if (response.Id == null) // notification
                    return NoContent();
                return Ok(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки MCP-запроса");
            return BadRequest(new McpResponse
            {
                Id = null,
                Error = new McpRpcError { Code = -32700, Message = "Parse error" }
            });
        }
    }

    private async Task<McpResponse> ProcessSingleRequestAsync(JsonElement requestJson, CancellationToken ct)
    {
        var request = JsonSerializer.Deserialize<McpRpcRequest>(requestJson);

        if (request?.JsonRpc != "2.0")
            return CreateErrorResponse(request?.Id, -32600, "Invalid Request");

        return request.Method switch
        {
            "initialize" => HandleInitialize(request),
            "list_tools" => HandleListTools(request),
            "call" => await HandleCallAsync(request, ct),
            _ => CreateErrorResponse(request.Id, -32601, "Method not found")
        };
    }

    private McpResponse HandleInitialize(McpRpcRequest request)
    {
        var result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                tools = new { dynamic_registration = false }
            }
        };
        return new McpResponse { Id = request.Id, Result = result };
    }

    private McpResponse HandleListTools(McpRpcRequest request)
    {
        var tools = _tools.Select(t => new
        {
            name = t.Name,
            description = t.Description,
            inputSchema = new
            {
                type = "object",
                properties = new Dictionary<string, string>(),
                required = Array.Empty<string>()
            }
        }).ToArray();

        return new McpResponse { Id = request.Id, Result = new { tools } };
    }

    private async Task<McpResponse> HandleCallAsync(McpRpcRequest request, CancellationToken ct)
    {
        try
        {
            if (request.Params.ValueKind != JsonValueKind.Object)
                return CreateErrorResponse(request.Id, -32602, "Invalid params");
            var toolName = request.Params.GetProperty("name").GetString();
            var argsElement = request.Params.GetProperty("arguments");

            if (argsElement.ValueKind != JsonValueKind.Object)
                return CreateErrorResponse(request.Id, -32602, "arguments must be object");

            var args = argsElement.EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");

            var tool = _tools.FirstOrDefault(t => t.Name == toolName);
            if (tool == null)
                return CreateErrorResponse(request.Id, -32601, $"Tool '{toolName}' not found");

            var result = await tool.ExecuteAsync(args, ct);
            return new McpResponse { Id = request.Id, Result = result };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка выполнения инструмента");
            return CreateErrorResponse(request.Id, -32000, ex.Message);
        }
    }

    private static McpResponse CreateErrorResponse(object? id, int code, string message)
        => new() { Id = id, Error = new McpRpcError { Code = code, Message = message } };
}