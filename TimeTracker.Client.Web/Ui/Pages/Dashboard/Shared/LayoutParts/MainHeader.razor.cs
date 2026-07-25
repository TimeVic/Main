using Fluxor;
using LumexUI;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Web.Services.UI;
using TimeTracker.Client.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainHeader
{
    private static readonly NavbarSlots NavbarClasses = new()
    {
        Base = "bg-white/95",
        Wrapper = "max-w-none px-4 sm:px-6 lg:px-8 gap-3",
        Content = "gap-2",
        Item = "shrink-0",
        Menu = "gap-1"
    };

    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    [Inject]
    public IState<WorkspaceState> WorkspaceState { get; set; }
    
    [Inject]
    public WorkspaceInitializationService _workspaceInitialization { get; set; }
    
    private bool _isShowAddWorkspaceModal = false;
    private bool IsWorkspaceCreationAvailable => WorkspaceState.Value.IsLoaded
        && WorkspaceState.Value.List.Count(item => item.IsCreatedByCurrentUser && item.IsDefault == false)
            < GlobalConstants.MaxActiveCreatedWorkspaces;

    private void OnSelectWorkspace(WorkspaceDto? workspace)
    {
        if (workspace == null)
        {
            // Clicked on selected item
            return;
        }
        _workspaceInitialization.ChangeWorkspace(workspace);
    }

    private Task OnClickLogout()
    {
        Dispatcher.Dispatch(new LogoutAction());
        return Task.CompletedTask;
    }

    private void OnNavigateToUserSettings()
    {
        NavigationManager.NavigateTo(UrlService.GetDashboardUrl("user/settings"));
    }
}
