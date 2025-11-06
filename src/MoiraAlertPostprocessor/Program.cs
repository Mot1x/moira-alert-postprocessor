using Microsoft.AspNetCore.Mvc;
using MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm;
using MoiraAlertPostprocessor.Infrastructure.NlServices;
using MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;
using MoiraAlertPostprocessor.Infrastructure.Repositories.Interfaces;
using MoiraAlertPostprocessor.Mapping;

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

builder.Services.Configure<TelegramAlertOptions>(
    builder.Configuration.GetSection(TelegramAlertOptions.SectionName)
);

builder.Services.AddHttpClient(); // для внутренних нужд Telegram.Bot (если понадобится)
builder.Services.AddSingleton<IMoiraAlertChannel, TelegramAlertChannel>();

builder.Services.AddAutoMapper(typeof(MoiraMappingProfile));

var app = builder.Build();

// Health-check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "ok", urls }));

app.MapControllers();
app.Run();