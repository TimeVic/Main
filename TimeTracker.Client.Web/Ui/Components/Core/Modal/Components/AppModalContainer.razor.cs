using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Components.Core.Modal.Components;

public partial class AppModalContainer : ComponentBase, IDisposable
{
    [Inject]
    public IAppModalDialogService ModalDialogService { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ModalDialogService.OnModalsChanged += HandleModalsChanged;
    }

    private void HandleModalsChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ModalDialogService.OnModalsChanged -= HandleModalsChanged;
    }
}
