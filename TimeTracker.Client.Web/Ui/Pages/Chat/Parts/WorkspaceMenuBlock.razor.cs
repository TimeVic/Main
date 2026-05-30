using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Chat.Parts;

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
