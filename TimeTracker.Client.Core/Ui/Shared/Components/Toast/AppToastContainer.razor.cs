using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Ui.Shared.Components;

public partial class AppToastContainer : ComponentBase, IDisposable
{
    [Inject]
    protected IToastService ToastService { get; set; } = default!;

    protected override void OnInitialized()
    {
        ToastService.OnToastsUpdated += HandleToastsUpdated;
    }

    private void HandleToastsUpdated()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ToastService.OnToastsUpdated -= HandleToastsUpdated;
    }
}
