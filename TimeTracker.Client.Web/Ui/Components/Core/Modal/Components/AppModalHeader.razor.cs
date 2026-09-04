using Microsoft.AspNetCore.Components;

namespace TimeTracker.Client.Web.Ui.Components.Core.Modal.Components;

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

    private bool ShowCloseButton => HasCloseButton && (ModalInstance == null || ModalInstance.Options.HasCloseButton);

    private async Task OnCloseClick()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
        ModalInstance?.Close(AppModalResult.Cancel("close_button"));
    }
}
