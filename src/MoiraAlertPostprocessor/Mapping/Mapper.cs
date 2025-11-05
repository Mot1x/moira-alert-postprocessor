using MoiraAlertPostprocessor.Domain.Entities;
using MoiraPostprocessor.API.Models;
using MoiraPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Mapping;

public static class Mapper
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

    public static Alert ToDomain(this IncomingAlertDto dto)
    {
        var metrics = (dto.Metrics ?? Enumerable.Empty<MetricDto>())
            .Select(m => new Metric(m.Name, new Dictionary<string, string>
            {
                ["value"] = m.Value.ToString(),
                ["unit"] = m.Unit ?? string.Empty
            }));
        return new Alert(dto.Id, dto.Name, dto.State, dto.Trigger, dto.Timestamp, metrics);
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