using MoiraAlertPostprocessor.Domain.Entities;
using MoiraPostprocessor.API.Models;
using MoiraPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Mapping;

public static class AlertMapper
{
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