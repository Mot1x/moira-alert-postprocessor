namespace MoiraAlertPostprocessor.Infrastructure.MCP;

public class McpClient : IToolExecuter
{
    private readonly ApiToolExecutor _apiToolExecutor;

    public McpClient(ApiToolExecutor apiToolExecutor)
    {
        _apiToolExecutor = apiToolExecutor;
    }

    public async Task<ToolExecutionResult> ExecuteAsync(string toolName, Dictionary<string, string> args,
        CancellationToken ct = default)
    {
        switch (toolName)
        {
            case "execute_on_api":
                return await _apiToolExecutor.ExecuteAsync(toolName, args, ct);
            default:
                throw new ArgumentException($"Unsupported tool: {toolName}");
        }
    }
}