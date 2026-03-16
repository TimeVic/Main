using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Ui.Pages.Chat.Parts;

public partial class WorkspaceMenuBlock
{
    [Inject]
    protected IState<WorkspaceState> WorkspaceState { get; set; }
    
    [Inject]
    protected IState<AuthState> AuthState { get; set; }

    [Inject]
    public WorkspaceInitializationService _workspaceInitialization { get; set; }
    
    private void OnWorkspaceChanged(Guid workspaceId)
    {
        var workspace = WorkspaceState.Value.List.First(x => x.Id == workspaceId);
        _workspaceInitialization.ChangeWorkspace(workspace);
    }
}
