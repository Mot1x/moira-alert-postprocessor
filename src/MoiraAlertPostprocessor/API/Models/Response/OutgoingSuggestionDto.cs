using System.Collections.Generic;

namespace MoiraAlertPostprocessor.API.Models.Response
{
    public class OutgoingSuggestionDto
    {
        public string Summary { get; set; }
        public string Details { get; set; }
        public List<string> Actions { get; set; }
        public string? AnalysisStatus { get; set; }
        public bool? IsActionableByMcp { get; set; }
    }
}