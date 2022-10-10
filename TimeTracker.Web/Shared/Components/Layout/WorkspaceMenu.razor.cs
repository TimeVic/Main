using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common.Actions;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Shared.Components.Layout;

public partial class WorkspaceMenu
{
    [Inject]
    public IState<WorkspaceState> _workpsaceState { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void OnClickToMenuItem(WorkspaceDto workspace)
    {
        if (workspace.Id == _authState.Value.Workspace.Id)
        {
            return;
        }

        Dispatcher.Dispatch(new SetWorkspaceAction(workspace));
        Dispatcher.Dispatch(new PersistDataAction());
    }
}
