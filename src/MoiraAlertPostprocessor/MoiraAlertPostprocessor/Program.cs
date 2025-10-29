using MoiraAlertPostprocessor.Domain.Interfaces;
using MoiraAlertPostprocessor.Infrastructure.GptOss;
using MoiraAlertPostprocessor.Infrastructure.Repositories;
using MoiraPostprocessor.Application.UseCases.ProcessAlert;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Configure gpt-oss options (appsettings or env)
var gptOptions = new GptOssOptions
{
    Endpoint = configuration.GetValue<string>("GptOss:Endpoint") ?? "http://localhost:8000/generate",
    Model = configuration.GetValue<string>("GptOss:Model") ?? "gpt-oss-small"
};
builder.Services.AddSingleton(gptOptions);

// Framework
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // keep default System.Text.Json camelCase
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ports -> Adapters
builder.Services.AddSingleton<IAlertRepository, InMemoryAlertRepository>();

builder.Services.AddHttpClient<GptOssClient>(c => { c.Timeout = TimeSpan.FromSeconds(30); });

// register INlpService resolving to GptOssClient
builder.Services.AddSingleton<INlpService>(sp => sp.GetRequiredService<GptOssClient>());

// UseCase and controllers
builder.Services.AddTransient<ProcessAlertUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();