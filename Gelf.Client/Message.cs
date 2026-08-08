namespace Gelf.Client;

public sealed class Message
{
    public string Version { get; } = "1.1";

    public string Host { get; set; } = Environment.MachineName;

    public DateTime Time { get; set; } = DateTime.UtcNow;

    public string ShortMessage { get; set; } = string.Empty;

    public ELevel Level { get; set; } = ELevel.Info;

    public IEnumerable<KeyValuePair<string, object?>> AdditionalFields { get; set; } = [];
}
