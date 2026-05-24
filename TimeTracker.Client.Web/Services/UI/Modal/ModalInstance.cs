namespace TimeTracker.Client.Web.Services.UI.Modal;

public class ModalInstance
{
    
    private readonly TaskCompletionSource<object?> _tcs;
    private readonly ModalDialogService _service;
    private readonly ModalEntry _entry;

    public ModalInstance(TaskCompletionSource<object?> tcs, ModalDialogService service, ModalEntry entry)
    {
        _tcs = tcs;
        _service = service;
        _entry = entry;
    }

    public void Close(object? result = null)
    {
        _tcs.TrySetResult(result);
        _service.Close(_entry);
    }
}
