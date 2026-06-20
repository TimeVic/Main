using System.Globalization;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Web.Services;
using TimeTracker.Client.Web.Services.UI;
using TimeTracker.Client.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainHeader
{
    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    [Inject]
    public IState<WorkspaceState> WorkspaceState { get; set; }
    
    [Inject]
    public WorkspaceInitializationService _workspaceInitialization { get; set; }
    
    private bool _isShowAddWorkspaceModal = false;
    private string CurrentLanguageKey => (AuthState.Value.User?.Language?.Code ?? CultureInfo.CurrentUICulture.Name) == ILocalizationUrlService.UkrainianCultureName
        ? "Ukrainian"
        : "English";

    private void OnSelectWorkspace(WorkspaceDto? workspace)
    {
        if (workspace == null)
        {
            // Clicked on selected item
            return;
        }
        _workspaceInitialization.ChangeWorkspace(workspace);
    }

    private void ToggleProfileMenu()
    {
        
    }

    private Task OnClickLogout()
    {
        Dispatcher.Dispatch(new LogoutAction());
        return Task.CompletedTask;
    }

    private void OnNavigateToUserSettings()
    {
        NavigationManager.NavigateTo(SiteUrl.Dashboard_User_Settings);
    }
    
    private async Task OnSelectLanguage(string cultureName)
    {
        if (AuthState.Value.User?.Id != Guid.Empty)
        {
            var user = await ApiService.UserUpdateSettingsAsync(new UpdateSettingsRequest
            {
                UserName = AuthState.Value.User?.UserName,
                LanguageCode = cultureName
            });
            if (user != null)
            {
                Dispatcher.Dispatch(new UpdateUserAction(user));
            }
        }

        await Js.InvokeVoidAsync("localStorage.setItem", ILocalizationUrlService.LocalStorageKey, cultureName);
        NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
    }
}
