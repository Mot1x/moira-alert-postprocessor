using MoiraPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Domain.Entities;

public class AlertEvent(
    string rawMetric,
    IDictionary<string, double> values,
    long unixTimestamp,
    bool triggerEvent,
    string state,
    string oldState)
{
    public string RawMetric { get; } = rawMetric;
    public Metric ParsedMetric { get; } = Metric.Parse(rawMetric);

    public IReadOnlyDictionary<string, double> Values { get; } = new Dictionary<string, double>(values);

    public DateTimeOffset Timestamp { get; } = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
    public bool TriggerEvent { get; } = triggerEvent;
    public string State { get; } = state;
    public string OldState { get; } = oldState;
}