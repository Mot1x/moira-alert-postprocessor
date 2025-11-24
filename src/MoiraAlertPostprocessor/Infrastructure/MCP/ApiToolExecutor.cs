using System.Net;
using System.Text;
using System.Text.Json;

namespace MoiraAlertPostprocessor.Infrastructure.MCP;

public class ApiToolExecutor : IToolExecuter
{
    private readonly HttpClient _httpClient;

    public ApiToolExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ToolExecutionResult> ExecuteAsync(string toolName, Dictionary<string, string> args,
        CancellationToken ct = default)
    {
        if (toolName != "execute_on_api")
            return new ToolExecutionResult(false, Error: "Поддерживается только инструмент 'execute_on_api'");

        try
        {
            var url = args["url"];
            var method = args["method"].ToUpperInvariant();
            var body = args["body"];

            // 🔒 ВАЛИДАЦИЯ URL
            if (!IsValidUrl(url, out var uri))
                return new ToolExecutionResult(false, Error: "URL не прошёл валидацию безопасности");

            // 🔒 ВАЛИДАЦИЯ МЕТОДА
            if (method is not ("GET" or "POST" or "PUT" or "DELETE"))
                return new ToolExecutionResult(false, Error: $"Метод {method} не разрешён");

            var request = new HttpRequestMessage(new HttpMethod(method), uri);

            // Тело запроса (если не GET)
            if (body != null && method != "GET")
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            // 🔒 Добавляем доверенные заголовки (не из LLM!)
            request.Headers.Add("User-Agent", "Moira-MCP-Agent/1.0");
            // request.Headers.Authorization = ... // если нужна аутентификация

            var response = await _httpClient.SendAsync(request, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            var resultMessage = $"HTTP {response.StatusCode}: {responseContent}";

            return new ToolExecutionResult(
                Success: response.IsSuccessStatusCode,
                Output: resultMessage,
                Error: response.IsSuccessStatusCode ? null : $"Статус: {response.StatusCode}"
            );
        }
        catch (Exception ex)
        {
            return new ToolExecutionResult(false, Error: $"Исключение: {ex.Message}");
        }
    }

    private bool IsValidUrl(string url, out Uri uri)
    {
        uri = null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var u))
            return false;

        // Только HTTP/HTTPS
        if (u.Scheme is not ("http" or "https"))
            return false;

        uri = u;
        return true;
    }
}