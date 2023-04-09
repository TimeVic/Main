using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Workspace;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common.Actions;
using TimeTracker.Web.Store.Workspace;

namespace TimeTracker.Web.Pages.Dashboard.Workspace;

public partial class WorkspaceChangingPage
{
    [Parameter]
    public string? PageRoute { get; set; }
    
    [Parameter]
    public long WorkspaceId { get; set; }

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
        
        await _workspaceInitializationService.Init(true);
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

        if (_authState.Value.Workspace == null || workspace.Id != _authState.Value.Workspace?.Id)
        {
            _workpsaceState.StateChanged -= OnWorkspaceListChanged;
            // The page was initialized early
            Dispatcher.Dispatch(new SetWorkspaceAction(workspace));
            Dispatcher.Dispatch(new PersistDataAction());
            _workspaceInitializationService.AfterInit(true).Wait();
        }
        NavigateTo();
    }

    private void NavigateTo()
    {
        _navigationManager.NavigateTo(
            string.IsNullOrEmpty(PageRoute) ? SiteUrl.DashboardBase : PageRoute  
        );
    }
}
