using MoiraAlertPostprocessor.Domain.Entities;
using MoiraPostprocessor.API.Models;

namespace MoiraAlertPostprocessor.Mapping;

public static class MoiraMapper
{
    public static MoiraAlert ToDomain(this IncomingMoiraWebhookDto dto)
    {
        var trigger = new Trigger(dto.Trigger?.Id, dto.Trigger?.Name, dto.Trigger?.Description,
            dto.Trigger?.Tags ?? Enumerable.Empty<string>());

        var events = (dto.Events ?? Enumerable.Empty<EventDto>())
            .Select(e => new AlertEvent(e.Metric, e.Values ?? new Dictionary<string, double>(), e.Timestamp,
                e.TriggerEvent, e.State, e.OldState))
            .ToList();

        var contact = dto.Contact == null
            ? null
            : new Contact(dto.Contact.Type, dto.Contact.Value, dto.Contact.Id, dto.Contact.User, dto.Contact.Team);

        return new MoiraAlert(trigger, events, contact, dto.Plot, dto.Plots ?? Enumerable.Empty<string>(),
            dto.Throttled);
    }

    public static OutgoingSuggestionDto ToDto(this Suggestion suggestion)
    {
        return new OutgoingSuggestionDto
        {
            Summary = suggestion.Summary,
            Details = suggestion.Details,
            Actions = suggestion.Actions?.ToList() ?? new List<string>()
        };
    }
}