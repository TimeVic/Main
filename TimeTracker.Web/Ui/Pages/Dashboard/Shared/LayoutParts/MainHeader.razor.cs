using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainHeader
{
    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    [Inject]
    public IState<WorkspaceState> WorkspaceState { get; set; }
    
    [Inject]
    public WorkspaceInitializationService _workspaceInitialization { get; set; }
    
    private void OnSelectWorkspace(WorkspaceDto? workspace)
    {
        if (workspace == null)
        {
            // Clicked on selected item
            return;
        }
        _workspaceInitialization.ChangeWorkspace(workspace);
    }

    private void ToggleProfileMenu()
    {
        
    }

    private Task OnClickLogout()
    {
        Dispatcher.Dispatch(new LogoutAction());
        return Task.CompletedTask;
    }
}
