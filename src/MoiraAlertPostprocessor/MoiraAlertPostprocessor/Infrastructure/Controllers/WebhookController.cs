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
        var outDto = MoiraMapper.ToDto(response.Suggestion);
        return Ok(outDto);
    }
}