using Prometheus;

namespace MoiraAlertPostprocessor.Infrastructure.Services;

public class FeedbackMetricsService
{
    private readonly Counter feedbackCounter = Metrics.CreateCounter(
        "moira_ai_feedback_total", 
        "User feedback on AI suggestions", 
        new CounterConfiguration
        {
            LabelNames = ["rating"]
        });

    public void RecordFeedback(string rating)
    {
        feedbackCounter.WithLabels(rating).Inc();
    }
}