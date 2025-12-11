using System;
using System.Collections.Generic;
using MoiraAlertPostprocessor.Core.Domain.Entities;
using MoiraAlertPostprocessor.Domain.Entities;

namespace MoiraPostprocessor.Domain.Entities
{
    public class Alert(
        string id,
        string name,
        string state,
        string trigger,
        DateTime timestamp,
        IEnumerable<Metric> metrics)
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public string State { get; } = state;
        public string Trigger { get; } = trigger;
        public DateTime Timestamp { get; } = timestamp;
        public IReadOnlyCollection<Metric> Metrics { get; } = new List<Metric>(metrics);
    }
}