using Microsoft.AspNetCore.Components;
using TimeTracker.Client.Web.Services.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared;

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
    }
}
