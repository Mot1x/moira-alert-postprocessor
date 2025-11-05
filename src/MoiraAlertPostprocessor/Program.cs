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

builder.Services.Configure<OllamaOptions>(configuration.GetSection("Ollama"))
    .AddSingleton<OllamaOptions>();

// Framework
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // keep default System.Text.Json camelCase
});
builder.Services.AddEndpointsApiExplorer();

// Ports -> Adapters
builder.Services.AddSingleton<IAlertRepository, MoiraAlertPostprocessor.Infrastructure.Repositories.InMemoryAlertRepository>();

// Register HTTP client for Ollama
builder.Services.AddHttpClient<OllamaClient>();

// Use Ollama as the only NLP provider
builder.Services.AddSingleton<INlpService>(sp => sp.GetRequiredService<OllamaClient>());

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