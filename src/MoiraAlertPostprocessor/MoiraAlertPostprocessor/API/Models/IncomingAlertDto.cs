using System;
using System.Collections.Generic;

namespace MoiraPostprocessor.API.Models
{
    // Пример DTO — ПРИМЕРНЫЙ. ПРИШЛИТЕ РЕАЛЬНЫЙ PAYLOAD МОИРА для точной структуры.
    public class IncomingAlertDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Trigger { get; set; }
        public DateTime Timestamp { get; set; }
        public List<MetricDto> Metrics { get; set; }
    }

    public class MetricDto
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
    }
}