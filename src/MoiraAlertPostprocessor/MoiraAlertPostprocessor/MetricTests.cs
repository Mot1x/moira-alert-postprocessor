using MoiraAlertPostprocessor.Domain.Entities;
using Xunit;

public class MetricTests
{
    [Fact]
    public void Parse_ShouldExtractNameAndLabels()
    {
        var raw = "user_requests_total;endpoint=/hello;instance=metric-spawner:8000;job=metric_spawner";
        var metric = Metric.Parse(raw);
        Assert.Equal("user_requests_total", metric.Name);
        Assert.Equal("/hello", metric.Labels["endpoint"]);
        Assert.Equal("metric-spawner:8000", metric.Labels["instance"]);
        Assert.Equal("metric_spawner", metric.Labels["job"]);
    }
}