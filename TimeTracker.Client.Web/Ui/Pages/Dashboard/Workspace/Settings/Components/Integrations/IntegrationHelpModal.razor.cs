using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Integrations;

public partial class IntegrationHelpModal
{
    [Parameter]
    public IntegrationHelpInfo? HelpInfo { get; set; }

    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    private async Task CloseModal()
    {
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
    }
}
