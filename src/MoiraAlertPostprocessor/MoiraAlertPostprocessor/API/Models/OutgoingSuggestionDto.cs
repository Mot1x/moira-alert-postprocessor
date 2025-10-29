using System.Collections.Generic;

namespace MoiraPostprocessor.API.Models
{
    public class OutgoingSuggestionDto
    {
        public string Summary { get; set; }
        public string Details { get; set; }
        public List<string> Actions { get; set; }
    }
}