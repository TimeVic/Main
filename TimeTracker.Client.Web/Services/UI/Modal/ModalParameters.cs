namespace TimeTracker.Client.Web.Services.UI.Modal;

public class ModalParameters
{
    internal Dictionary<string, object> Parameters { get; } = new();

    public object this[string key]
    {
        get => Parameters[key];
        set => Parameters[key] = value;
    }

}
