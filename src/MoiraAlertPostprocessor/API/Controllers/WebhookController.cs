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
public class WebhookController : ControllerBase
{
    private readonly ProcessAlertUseCase _processAlertUseCase;
    private readonly IMapper _mapper;
    private readonly IMoiraAlertChannel _moiraAlertChannel;

    public WebhookController(
        ProcessAlertUseCase processAlertUseCase,
        IMapper mapper,
        IMoiraAlertChannel moiraAlertChannel)
    {
        _processAlertUseCase = processAlertUseCase;
        _mapper = mapper;
        _moiraAlertChannel = moiraAlertChannel;
    }

    [HttpPost("alert")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OutgoingSuggestionDto), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> ReceiveAlert([FromBody] IncomingMoiraWebhookDto dto,
        CancellationToken cancellationToken)
    {
        var domain = _mapper.Map<MoiraAlert>(dto);
        var response = await _processAlertUseCase.ExecuteAsync(new ProcessAlertRequest(domain), cancellationToken);
        var outDto = _mapper.Map<OutgoingSuggestionDto>(response.Suggestion);

        await _moiraAlertChannel.AlertUsersAsync(outDto); // тут надо с типами работать мне некогда

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