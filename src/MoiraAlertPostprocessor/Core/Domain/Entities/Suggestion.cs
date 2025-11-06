namespace MoiraAlertPostprocessor.Domain.Entities;

public class Suggestion(string summary, string details, IEnumerable<string>? actions = null)
{
    public string Summary { get; } = summary;
    public string Details { get; } = details;
    public IReadOnlyCollection<string> Actions { get; } = actions == null
        ? new List<string>()
        : new List<string>(actions);
}