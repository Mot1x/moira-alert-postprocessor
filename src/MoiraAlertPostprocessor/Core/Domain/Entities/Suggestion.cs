namespace MoiraAlertPostprocessor.Domain.Entities;

public class Suggestion
{
    public Suggestion(string summary, string details, IEnumerable<string>? actions = null,
        string? analysisStatus = null, bool? isActionable = null, string? executionResult = null)
    {
        Summary = summary;
        Details = details;
        ExecutionResult = executionResult;
        Actions = actions == null ? new List<string>() : new List<string>(actions);
        AnalysisStatus = analysisStatus;
        IsActionableByMcp = isActionable;
    }

    public string Summary { get; }
    public string Details { get; }
    public IReadOnlyCollection<string> Actions { get; }
    public string? AnalysisStatus { get; }
    public bool? IsActionableByMcp { get; }
    public string? ExecutionResult { get; }
}