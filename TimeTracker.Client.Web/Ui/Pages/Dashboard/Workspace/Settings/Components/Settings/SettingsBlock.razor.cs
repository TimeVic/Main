using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Common;
using TimeTracker.Client.Core.Store.Workspace;

using TimeTracker.Client.Web.Services.UI;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Modals;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Settings;

public partial class SettingsBlock
{
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<WorkspaceState> _workspaceState { get; set; }

    [Inject]
    private IModalDialogProviderService _modalDialogService { get; set; } = null!;
    
    private WorkspaceDto? _workspace => _authState.Value.Workspace;

    private async Task OpenAddWorkspaceModal()
    {
        await _modalDialogService.ShowAddWorkspaceModal();
    }
}
