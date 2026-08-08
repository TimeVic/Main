namespace TimeTracker.Business.Logging.Client.GrayLog;

public interface IGraylogGelfClient
{
    void Send(
        string message,
        ICollection<KeyValuePair<string, object?>> fields,
        object? additionalDataObject = null
    );
}
