
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.Workspace;

namespace TimeTracker.Web.Pages.Dashboard.Shared;

public partial class Layout
{
    [Inject]
    private WorkspaceInitializationService _workspaceInitializationService { get; set; }
    
    [Inject]
    private NavigationManager _navigationManager { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = true;
        await base.OnInitializedAsync();
        if (AuthState.Value.IsLoggedIn)
        {
            await OnLoggedInAsync();
        }
    }
    
    protected override async Task OnLoggedInAsync()
    {
        if (!_navigationManager.GetPath().StartsWith("/board-change/"))
        {
            await _workspaceInitializationService.Init();
            await _workspaceInitializationService.AfterInit();
        }
        await Task.CompletedTask;
    }
}
