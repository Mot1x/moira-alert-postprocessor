using MoiraPostprocessor.Domain.Entities;

namespace MoiraAlertPostprocessor.Domain.Entities;

public class MoiraAlert(
    Trigger trigger,
    IEnumerable<AlertEvent> events,
    Contact contact,
    string plot,
    IEnumerable<string> plots,
    bool throttled)
{
    public Trigger Trigger { get; } = trigger;
    public IReadOnlyCollection<AlertEvent> Events { get; } = new List<AlertEvent>(events);
    public Contact Contact { get; } = contact;
    public string Plot { get; } = plot;
    public IReadOnlyCollection<string> Plots { get; } = new List<string>(plots);
    public bool Throttled { get; } = throttled;
}