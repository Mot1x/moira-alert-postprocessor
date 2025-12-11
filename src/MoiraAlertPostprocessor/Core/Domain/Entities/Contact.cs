namespace MoiraAlertPostprocessor.Core.Domain.Entities
{
    public class Contact(string type, string value, string id, string user, string team)
    {
        public string Type { get; } = type;
        public string Value { get; } = value;
        public string Id { get; } = id;
        public string User { get; } = user;
        public string Team { get; } = team;
    }
}