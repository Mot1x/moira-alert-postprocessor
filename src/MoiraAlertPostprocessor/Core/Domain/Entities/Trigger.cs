namespace MoiraAlertPostprocessor.Domain.Entities
{
    public class Trigger(string id, string name, string description, IEnumerable<string> tags)
    {
        public string Id { get; } = id;
        public string Name { get; } = name;
        public string Description { get; } = description;
        public IReadOnlyCollection<string> Tags { get; } = new List<string>(tags);
    }
}