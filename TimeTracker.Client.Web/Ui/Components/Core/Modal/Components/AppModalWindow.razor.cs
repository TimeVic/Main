using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TimeTracker.Client.Web.Ui.Components.Core.Modal.Components;

public partial class AppModalWindow : ComponentBase
{
    [Parameter]
    public required AppModalInstance ModalInstance { get; set; }

    private string _sizeClass => ModalInstance.Options.Size switch
    {
        AppModalSize.Small => "max-w-md",
        AppModalSize.Medium => "max-w-lg",
        AppModalSize.Large => "max-w-2xl",
        AppModalSize.ExtraLarge => "max-w-4xl",
        AppModalSize.Full => "max-w-full m-4",
        _ => "max-w-md"
    };

    private void OnBackdropClick()
    {
        if (ModalInstance.Options.IsCloseOnBackdropClick)
        {
            ModalInstance.Close(AppModalResult.Cancel("backdrop"));
        }
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (ModalInstance.Options.IsCloseOnEscapeKey && e.Key == "Escape")
        {
            ModalInstance.Close(AppModalResult.Cancel("escape"));
        }
    }
}
