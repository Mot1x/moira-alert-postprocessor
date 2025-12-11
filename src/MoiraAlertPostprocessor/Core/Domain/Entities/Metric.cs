using System.Text;

namespace MoiraAlertPostprocessor.Core.Domain.Entities;

public class Metric(string name, IDictionary<string, string> labels)
{
    public string Name { get; } = name;

    public IReadOnlyDictionary<string, string> Labels { get; } = new Dictionary<string, string>(labels);

    // Parse strings like:
    // "user_requests_total;endpoint=/hello;instance=metric-spawner:8000;job=metric_spawner"
    public static Metric Parse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new Metric("unknown", new Dictionary<string, string>());

        var parts = raw.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var name = parts.Length > 0 
            ? parts[0] 
            : raw;
        
        var labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < parts.Length; i++)
        {
            var p = parts[i];
            var idx = p.IndexOf('=');
            if (idx > 0)
            {
                var k = p.Substring(0, idx).Trim();
                var v = p.Substring(idx + 1).Trim();
                labels[k] = v;
            }
            else
            {
                labels[$"label{i}"] = p.Trim();
            }
        }

        return new Metric(name, labels);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Name);
        if (Labels != null && Labels.Any())
        {
            sb.Append(" { ");
            sb.Append(string.Join(", ", Labels.Select(kv => $"{kv.Key}=\"{kv.Value}\"")));
            sb.Append(" }");
        }

        return sb.ToString();
    }
}