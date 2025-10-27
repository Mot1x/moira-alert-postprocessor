using MoiraAlertPostprocessor.Logic.UseCases.MoiraAlertReceive;
using MoiraAlertPostprocessor.Logic.UseCases.MoiraAlertReceive.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register custom services
builder.Services.AddScoped<IAlertProcessingService, AlertProccessingService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();