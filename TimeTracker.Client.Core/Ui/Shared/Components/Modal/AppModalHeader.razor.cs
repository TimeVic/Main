using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Core.Ui.Shared.Components.Modal;

public partial class AppModalHeader : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public bool HasCloseButton { get; set; } = true;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [CascadingParameter]
    public AppModal? DeclarativeModal { get; set; }

    private bool ShowCloseButton => HasCloseButton
        && (ModalInstance == null || ModalInstance.Options.HasCloseButton)
        && (DeclarativeModal == null || DeclarativeModal.HasCloseButton);

    private async Task OnCloseClick()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
        if (DeclarativeModal != null)
        {
            await DeclarativeModal.CloseAsync();
        }
        ModalInstance?.Close(AppModalResult.Cancel("close_button"));
    }
}
