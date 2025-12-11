using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoiraAlertPostprocessor.API.Models.Request;
using MoiraAlertPostprocessor.API.Models.Response;
using MoiraAlertPostprocessor.Domain.Entities;
using MoiraAlertPostprocessor.Infrastructure.MoiraAlertChannels;
using MoiraPostprocessor.Application.UseCases.ProcessAlert;

namespace MoiraAlertPostprocessor.API.Controllers;

[ApiController]
[Route("moira")]
public class WebhookController(
    ProcessAlertUseCase processAlertUseCase,
    IMapper mapper,
    IMoiraAlertChannel moiraAlertChannel)
    : ControllerBase
{
    [HttpPost("alert")]
    [ProducesResponseType(typeof(OutgoingSuggestionDto), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> ReceiveAlert([FromBody] IncomingMoiraWebhookDto dto,
        CancellationToken cancellationToken)
    {
        var domain = mapper.Map<MoiraAlert>(dto);
        var response = await processAlertUseCase.ExecuteAsync(new ProcessAlertRequest(domain), cancellationToken);
        var outDto = mapper.Map<OutgoingSuggestionDto>(response.Suggestion);

        // Отправляем в канал исходный триггер как JSON-файл, а не ответ нейросети
        await moiraAlertChannel.AlertUsersAsync(JsonContent.Create(dto), cancellationToken);

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