using MoiraAlertPostprocessor.Domain.Interfaces;
using MoiraAlertPostprocessor.Infrastructure.Ollama;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Ensure predictable listen URL (fallback to 8080)
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
           ?? configuration["ASPNETCORE_URLS"]
           ?? "http://localhost:8080";
builder.WebHost.UseUrls(urls);

// Configure Ollama options (appsettings or env)
var ollamaOptions = new OllamaOptions
{
    Endpoint = configuration.GetValue<string>("Ollama:Endpoint") ?? "http://localhost:11434/api/generate",
    Model = configuration.GetValue<string>("Ollama:Model") ?? "gpt-oss:20b-cloud",
    TimeoutSeconds = configuration.GetValue<int?>("Ollama:TimeoutSeconds") ?? 30
};
builder.Services.AddSingleton(ollamaOptions);

// Framework
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // keep default System.Text.Json camelCase
});
builder.Services.AddEndpointsApiExplorer();

// Ports -> Adapters
builder.Services.AddSingleton<IAlertRepository, MoiraAlertPostprocessor.Infrastructure.Repositories.InMemoryAlertRepository>();

// Register HTTP client for Ollama
builder.Services.AddHttpClient<OllamaClient>(c => { c.Timeout = TimeSpan.FromSeconds(ollamaOptions.TimeoutSeconds); });

// Use Ollama as the only NLP provider
builder.Services.AddSingleton<INlpService>(sp => sp.GetRequiredService<OllamaClient>());
Console.WriteLine($"[Startup] NLP provider: Ollama, endpoint={ollamaOptions.Endpoint}, model={ollamaOptions.Model}");

// UseCase and controllers
builder.Services.AddTransient<MoiraPostprocessor.Application.UseCases.ProcessAlert.ProcessAlertUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Health-check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "ok", urls }));

app.MapControllers();
app.Run();