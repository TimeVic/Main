using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Settings.Parts;

public partial class WorkspaceModeInfoBlock
{
    [Parameter]
    public EventCallback OnCreateWorkspaceClicked { get; set; }

    private WorkspaceMode? Mode => AuthState.Value.Workspace?.Mode;

    private async Task HandleCreateWorkspaceClick()
    {
        await OnCreateWorkspaceClicked.InvokeAsync();
    }
}

