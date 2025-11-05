using Microsoft.AspNetCore.Mvc;
using MoiraAlertPostprocessor.Mapping;
using MoiraPostprocessor.API.Models;
using MoiraPostprocessor.Application.UseCases.ProcessAlert;

namespace MoiraAlertPostprocessor.Infrastructure.Controllers;

[ApiController]
[Route("moira")]
public class WebhookController(ProcessAlertUseCase processAlertUseCase) : ControllerBase
{
    [HttpPost("alert")]
    public async Task<IActionResult> ReceiveAlert([FromBody] IncomingMoiraWebhookDto dto,
        CancellationToken cancellationToken)
    {
        // Note: consider validating a shared secret/header to ensure request from Moira.
        var domain = dto.ToDomain();
        var response = await processAlertUseCase.ExecuteAsync(new ProcessAlertRequest(domain), cancellationToken);
        var outDto = response.Suggestion.ToDto();

        // Log suggestion to console so it's visible when running the app
        try
        {
            Console.WriteLine("--- NLP Suggestion ---");
            Console.WriteLine("Summary: " + outDto.Summary);
            Console.WriteLine("Details:\n" + outDto.Details);
            if (outDto.Actions != null && outDto.Actions.Any())
            {
                Console.WriteLine("Actions:");
                foreach (var a in outDto.Actions)
                    Console.WriteLine(" - " + a);
            }

            Console.WriteLine("--- End Suggestion ---");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to log suggestion: " + ex.Message);
        }

        return Ok(outDto);
    }
}