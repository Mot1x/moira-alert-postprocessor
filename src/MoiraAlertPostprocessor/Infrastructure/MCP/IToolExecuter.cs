namespace MoiraAlertPostprocessor.Infrastructure.MCP;

public record ToolExecutionResult(bool Success, string? Output = null, string? Error = null);

public interface IToolExecuter
{
    public Task<ToolExecutionResult> ExecuteAsync(string toolName, Dictionary<string, string> args, CancellationToken ct = default);
}