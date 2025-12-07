namespace MoiraAlertPostprocessor.Infrastructure.MCP;

public interface IMcpTool
{
    string Name { get; }
    string Description { get; }
    Task<ToolExecutionResult> ExecuteAsync(Dictionary<string, string> arguments, CancellationToken ct = default);
}