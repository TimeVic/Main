using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace;

public partial class WorkspaceChangingPage
{
    [Parameter]
    public string? PageRoute { get; set; }
    
    [Parameter]
    public Guid WorkspaceId { get; set; }

    [Inject]
    private WorkspaceInitializationService _workspaceInitializationService { get; set; }
    
    [Inject]
    public IState<AuthState> _authState { get; set; }
    
    [Inject]
    public IState<WorkspaceState> _workpsaceState { get; set; }
    
    [Inject]
    private NavigationManager _navigationManager { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        if (_workpsaceState.Value.List.Any() && WorkspaceId == _authState.Value.Workspace?.Id)
        {
            NavigateTo();
            return;
        }
        
        _workspaceInitializationService.Init(true);
        _workpsaceState.StateChanged += OnWorkspaceListChanged;
        await base.OnInitializedAsync();
    }

    private void OnWorkspaceListChanged(object? sender, EventArgs e)
    {
        if (!_workpsaceState.Value.List.Any())
        {
            return;
        }

        var workspace = _workpsaceState.Value.List.FirstOrDefault(item => item.Id == WorkspaceId);
        if (workspace == null)
        {
            _navigationManager.NavigateTo(SiteUrl.Error404);
            return;
        }
        
        _workpsaceState.StateChanged -= OnWorkspaceListChanged;
        _workspaceInitializationService.ChangeWorkspace(workspace);
        NavigateTo();
    }

    private void NavigateTo()
    {
        _navigationManager.NavigateTo(
            string.IsNullOrEmpty(PageRoute) ? SiteUrl.DashboardBase : PageRoute,
            replace: true
        );
    }
}
