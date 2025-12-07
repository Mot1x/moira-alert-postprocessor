namespace MoiraAlertPostprocessor.Infrastructure.MCP;

public class McpClient
{
    private readonly IEnumerable<IMcpTool> _tools;
    public McpClient(HttpClient httpClient, IEnumerable<IMcpTool> tools)
    {
        _tools = tools;
    }

    public async Task<ToolExecutionResult> ForwardTool(string toolName,
        Dictionary<string, string> args,
        CancellationToken ct = default)
    {
        var tool = _tools.FirstOrDefault(t => t.Name == toolName);
        if (tool == null) throw new ArgumentException($"Unknown tool - {toolName}");
        return await tool.ExecuteAsync(args, ct);
    }
}