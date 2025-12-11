using System.Globalization;
using AutoMapper;
using MoiraAlertPostprocessor.API.Models.Request;
using MoiraAlertPostprocessor.API.Models.Response;
using MoiraAlertPostprocessor.Core.Domain.Entities;
using MoiraAlertPostprocessor.Domain.Entities;
using MoiraPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Infrastructure.Mapping;

public class MoiraMappingProfile : Profile
{
    public MoiraMappingProfile()
    {
        CreateMap<IncomingMoiraWebhookDto, MoiraAlert>()
            .ConvertUsing((src, ctx) => new MoiraAlert(
                trigger: MapTrigger(src.Trigger),
                events: MapEvents(src.Events),
                contact: MapContact(src.Contact),
                plot: src.Plot,
                plots: src.Plots ?? Enumerable.Empty<string>(),
                throttled: src.Throttled
            ));

        CreateMap<IncomingAlertDto, Alert>()
            .ConvertUsing((src, ctx) => new Alert(
                id: src.Id,
                name: src.Name,
                state: src.State,
                trigger: src.Trigger,
                timestamp: src.Timestamp,
                metrics: MapMetrics(src.Metrics)
            ));

        CreateMap<Suggestion, OutgoingSuggestionDto>()
            .ForMember(dest => dest.Summary, opt => opt.MapFrom(src => src.Summary))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.Actions, opt => opt.MapFrom(src => src.Actions ?? Enumerable.Empty<string>()))
            .ForMember(dest => dest.AnalysisStatus, opt => opt.MapFrom(src => src.AnalysisStatus))
            .ForMember(dest => dest.IsActionableByMcp, opt => opt.MapFrom(src => src.IsActionableByMcp));
    }

    private static Trigger? MapTrigger(TriggerDto? dto) =>
        dto == null
            ? null
            : new Trigger(dto.Id, dto.Name, dto.Description,
                dto.Tags ?? Enumerable.Empty<string>());

    private static IReadOnlyList<AlertEvent> MapEvents(IEnumerable<EventDto>? events) =>
        events?.Select(MapAlertEvent).ToList() ?? new List<AlertEvent>();

    private static AlertEvent MapAlertEvent(EventDto e) =>
        new AlertEvent(
            rawMetric: e.Metric,
            values: e.Values ?? new Dictionary<string, double>(),
            unixTimestamp: e.Timestamp,
            triggerEvent: e.TriggerEvent,
            state: e.State,
            oldState: e.OldState
        );

    private static Contact? MapContact(ContactDto? dto) =>
        dto == null
            ? null
            : new Contact(dto.Type, dto.Value, dto.Id, dto.User, dto.Team);

    private static IReadOnlyList<Metric> MapMetrics(IEnumerable<MetricDto>? dtos) =>
        dtos?.Select(MapMetric).ToList() ?? new List<Metric>();

    private static Metric MapMetric(MetricDto m) =>
        new Metric(
            name: m.Name,
            labels: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["value"] = m.Value.ToString(CultureInfo.InvariantCulture),
                ["unit"] = m.Unit ?? string.Empty
            }
        );
}