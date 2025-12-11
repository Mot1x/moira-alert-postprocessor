using MoiraAlertPostprocessor.Core.Domain.Entities.MoiraAlertChannel.Telegram;
using MoiraAlertPostprocessor.Infrastructure.Mapping;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Interfaces;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels.Telegramm.Services;
using MoiraAlertPostprocessor.Infrastructure.NlServices;
using MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;
using MoiraAlertPostprocessor.Infrastructure.Repositories;
using MoiraAlertPostprocessor.Infrastructure.Repositories.Interfaces;
using MoiraAlertPostprocessor.Infrastructure.Services;
using MoiraPostprocessor.Application.UseCases.ProcessAlert;
using Prometheus;

var builder = WebApplication.CreateBuilder();

var configuration = builder.Configuration;

// Ensure predictable listen URL (fallback to 8080)
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
           ?? configuration["ASPNETCORE_URLS"]
           ?? "http://localhost:8080";
builder.WebHost.UseUrls(urls);

builder.Services.Configure<OllamaOptions>(configuration.GetSection("Ollama"))
    .AddSingleton<OllamaOptions>();

// Framework
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Ports -> Adapters
builder.Services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();

// Register HTTP client for Ollama
builder.Services.AddHttpClient<OllamaClient>();

// NLP provider registration with safe fallback
var ollamaSection = configuration.GetSection("Ollama");
var ollamaEndpoint = ollamaSection["Endpoint"];
var ollamaModel = ollamaSection["Model"];

if (!string.IsNullOrWhiteSpace(ollamaEndpoint) && !string.IsNullOrWhiteSpace(ollamaModel))
    builder.Services.AddSingleton<INlpService>(sp => sp.GetRequiredService<OllamaClient>());
else
    builder.Services.AddSingleton<INlpService, SafeFallbackNlpService>();

// UseCase and controllers
builder.Services.AddTransient<ProcessAlertUseCase>();
builder.Services.AddSingleton<IVoteRepository, InMemoryVoteRepository>();
builder.Services.AddSingleton<FeedbackMetricsService>();

builder.Services.Configure<TelegramAlertOptions>(
    builder.Configuration.GetSection(TelegramAlertOptions.SectionName)
);

builder.Services.AddHttpClient(); // для внутренних нужд Telegram.Bot (если понадобится)

// Telegram parsing/formatting services
builder.Services.AddSingleton<ITelegramPostParser, JsonMoiraAlertTelegramPostParser>();
builder.Services.AddSingleton<ITelegramReplyFormatter, TelegramReplyFormatter>();

// Telegram channel registration with safe fallback
var tgSection = configuration.GetSection(TelegramAlertOptions.SectionName);
var tgToken = tgSection["BotToken"];
var tgChatId = tgSection["ChatId"];

if (!string.IsNullOrWhiteSpace(tgToken) && !string.IsNullOrWhiteSpace(tgChatId))
{
    builder.Services.AddSingleton<IMoiraAlertChannel, TelegramAlertChannel>();
    builder.Services.AddHostedService<TelegramUpdateWorker>();
}
else
{
    builder.Services.AddSingleton<IMoiraAlertChannel, NullAlertChannel>();
}

builder.Services.AddAutoMapper(cfg => { cfg.AddProfile<MoiraMappingProfile>(); });

var app = builder.Build();

app.UseMetricServer();
app.UseHttpMetrics();

// Health-check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "ok", urls }));

app.MapControllers();
app.Run();