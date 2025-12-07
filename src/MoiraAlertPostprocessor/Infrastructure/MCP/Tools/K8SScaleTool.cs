using System.Text.Json;

namespace MoiraAlertPostprocessor.Infrastructure.MCP.Tools;
//это просто пример возможной реализации тула, если не готовыми серверами пользоваться
public class K8SScaleTool : IMcpTool
{
    private readonly HttpClient _k8SClient;

    public string Name => "k8s_scale";
    public string Description => "Масштабирует Deployment в Kubernetes";

    public K8SScaleTool(HttpClient k8SClient) => _k8SClient = k8SClient;

    public async Task<ToolExecutionResult> ExecuteAsync(Dictionary<string, string> args, CancellationToken ct = default)
    {
        if (!args.TryGetValue("namespace", out var ns) ||
            !args.TryGetValue("name", out var name) ||
            !args.TryGetValue("replicas", out var replicasStr))
        {
            throw new ArgumentException("Требуются: namespace, name, replicas");
        }

        if (!int.TryParse(replicasStr, out var replicas) || replicas < 0)
            throw new ArgumentException("replicas должен быть неотрицательным числом");
        
        var patch = new { spec = new { replicas } };
        var json = JsonSerializer.Serialize(patch);
        var content = new StringContent(json, null, "application/merge-patch+json");

        using var response = await _k8SClient.PatchAsync(
            $"/apis/apps/v1/namespaces/{ns}/deployments/{name}", content, ct);
        response.EnsureSuccessStatusCode();

        return new ToolExecutionResult(true, $"Deployment {name} scaled to {replicas} replicas in {ns}");
    }
}