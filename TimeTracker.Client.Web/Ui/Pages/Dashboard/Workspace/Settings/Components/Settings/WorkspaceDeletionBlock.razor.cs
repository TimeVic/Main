using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;
using TimeTracker.Client.Web.Services.UI;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Settings;

public partial class WorkspaceDeletionBlock
{
    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = default!;

    private WorkspaceDto? Workspace => AuthState.Value.Workspace;

    private bool IsDeletionAvailable => Workspace?.IsWorkspaceOwner == true
        && Workspace.IsDefault == false;

    private void OpenConfirmation()
    {
        if (Workspace != null)
        {
            _modalDialogService.ShowDeleteWorkspaceModal(Workspace);
        }
    }
}
