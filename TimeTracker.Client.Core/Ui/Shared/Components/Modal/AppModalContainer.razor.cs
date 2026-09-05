using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Modal;

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
