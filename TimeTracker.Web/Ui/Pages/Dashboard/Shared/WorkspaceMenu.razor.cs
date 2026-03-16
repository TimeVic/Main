using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared;

public partial class WorkspaceMenu
{
    [Inject]
    public IState<WorkspaceState> _workpsaceState { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public UrlService _urlService { get; set; }

    [Inject]
    public WorkspaceInitializationService _workspaceInitialization { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void OnClickToMenuItem(WorkspaceDto? workspace)
    {
        if (workspace == null)
        {
            // Clicked on selected item
            return;
        }
        _workspaceInitialization.ChangeWorkspace(workspace);
    }
    
    private void OnManageWorkspaces()
    {
        NavigationManager.NavigateTo(SiteUrl.Workspace_List);
    }
}
