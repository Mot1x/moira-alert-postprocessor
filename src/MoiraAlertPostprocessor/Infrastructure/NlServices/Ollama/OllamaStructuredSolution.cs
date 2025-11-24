using System.Text.Json.Serialization;

namespace MoiraAlertPostprocessor.Infrastructure.NlServices.Ollama;

public record OllamaStructuredSolution(
    [property: JsonPropertyName("analysis_status")]
    string? AnalysisStatus,
    [property: JsonPropertyName("problem_summary")]
    string? ProblemSummary,
    [property: JsonPropertyName("suggested_solution")]
    SuggestedSolution? SuggestedSolution,
    [property: JsonPropertyName("is_actionable_by_mcp")]
    bool? IsActionableByMcp,
    [property: JsonPropertyName("confidence")]
    double? Confidence
);