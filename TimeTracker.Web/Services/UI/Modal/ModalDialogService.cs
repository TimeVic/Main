namespace TimeTracker.Web.Services.UI.Modal;

public class ModalDialogService
{
    
    public event Action OnChange;
    private readonly List<ModalEntry> _modals = new();
    public IReadOnlyList<ModalEntry> Modals => _modals;

    public Task<object?> ShowAsync<TComponent>(ModalParameters parameters = null) where TComponent : IModalComponent
    {
        var tcs = new TaskCompletionSource<object?>();
        var entry = new ModalEntry { Tcs = tcs };

        entry.Fragment = builder =>
        {
            var seq = 0;
            builder.OpenComponent(seq++, typeof(TComponent));

            if (parameters != null)
            {
                foreach (var p in parameters.Parameters)
                {
                    builder.AddAttribute(seq++, p.Key, p.Value);
                }
            }

            builder.AddAttribute(seq++, "ModalInstance", new ModalInstance(tcs, this, entry));
            builder.CloseComponent();
        };

        _modals.Add(entry);
        OnChange?.Invoke();

        return tcs.Task;
    }

    internal void Close(ModalEntry entry)
    {
        _modals.Remove(entry);
        OnChange?.Invoke();
    }

}
