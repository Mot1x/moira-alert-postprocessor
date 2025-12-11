namespace MoiraAlertPostprocessor.Core.Domain.Entities;

public class Suggestion(
    string summary,
    string details,
    IEnumerable<string>? actions = null,
    string? analysisStatus = null,
    bool? isActionable = null,
    string? solutionType = null,
    string? solutionDescription = null,
    string? solutionCommand = null)
{
    public string Summary { get; } = summary;
    public string Details { get; } = details;
    public IReadOnlyCollection<string> Actions { get; } =
        actions == null
            ? new List<string>()
            : new List<string>(actions);
    public string? AnalysisStatus { get; } = analysisStatus;
    public bool? IsActionableByMcp { get; } = isActionable;
    public string? SolutionType { get; } = solutionType;
    public string? SolutionDescription { get; } = solutionDescription;
    public string? SolutionCommand { get; } = solutionCommand;
}