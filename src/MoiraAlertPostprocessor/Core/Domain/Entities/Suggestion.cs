namespace MoiraAlertPostprocessor.Domain.Entities;

public class Suggestion
{
    public Suggestion(string summary, string details, IEnumerable<string>? actions = null,
        string? analysisStatus = null, bool? isActionable = null,
        string? solutionType = null, string? solutionDescription = null, string? solutionCommand = null)
    {
        Summary = summary;
        Details = details;
        Actions = actions == null ? new List<string>() : new List<string>(actions);
        AnalysisStatus = analysisStatus;
        IsActionableByMcp = isActionable;
        SolutionType = solutionType;
        SolutionDescription = solutionDescription;
        SolutionCommand = solutionCommand;
    }

    public string Summary { get; }
    public string Details { get; }
    public IReadOnlyCollection<string> Actions { get; }
    public string? AnalysisStatus { get; }
    public bool? IsActionableByMcp { get; }
    public string? SolutionType { get; }
    public string? SolutionDescription { get; }
    public string? SolutionCommand { get; }
}