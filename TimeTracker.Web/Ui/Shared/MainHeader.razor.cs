using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Extensions;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Common;
using TimeTracker.Web.Store.Ui;

namespace TimeTracker.Web.Ui.Shared;

public partial class MainHeader
{
    [Inject]
    private NavigationManager NavigationManager { get; set; }
    
    [Inject]
    private IState<AuthState> AuthState { get; set; }

    [Inject]
    private IState<CommonState> CommonState { get; set; }
    
    private UserDto _user
    {
        get => AuthState.Value.User;
    }
    
    private WorkspaceDto _workspace
    {
        get => AuthState.Value.Workspace;
    }
    
    private bool _isShowDashboardLink
    {
        get => !NavigationManager.GetPath().StartsWith(SiteUrl.DashboardBase);
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    
    private async Task OnSelectLanguageAsync(string id)
    {
        NavigationManager.NavigateTo("/", forceLoad: true);
    }
    
    private void OnLogout()
    {
        Dispatcher.Dispatch(new LogoutAction());
        NavigationManager.NavigateTo("/", true);
    }
    
    private void Logout()
    {
        Dispatcher.Dispatch(new LogoutAction());
        NavigationManager.NavigateTo("/", true);
    }

    private void ToggleMenu()
    {
        Dispatcher.Dispatch(new ToggleMainMenuAction());
    }
}
