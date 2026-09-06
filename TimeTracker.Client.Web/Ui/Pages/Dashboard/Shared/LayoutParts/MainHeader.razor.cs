using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Constants;
using TimeTracker.Client.Web.Services.UI;
using TimeTracker.Client.Web.Services.Workspace;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Permissions;
using TimeTracker.Client.Core.Store.Workspace;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members.Parts;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.LayoutParts;

public partial class MainHeader : IDisposable
{
    private bool _isMobileMenuOpen;

    private void ToggleMobileMenu()
    {
        _isMobileMenuOpen = !_isMobileMenuOpen;
    }

    [Inject]
    public IState<AuthState> AuthState { get; set; } = null!;
    
    [Inject]
    public IState<WorkspaceState> WorkspaceState { get; set; } = null!;

    [Inject]
    public WorkspaceInitializationService _workspaceInitialization { get; set; } = null!;

    [Inject]
    private ISecurityManager SecurityManager { get; set; } = null!;

    [Inject]
    private IState<WorkspacePermissionsState> WorkspacePermissionsState { get; set; } = null!;
    
    [Inject]
    private IModalDialogProviderService ModalDialogService { get; set; } = null!;

    private bool IsWorkspaceCreationAvailable => WorkspaceState.Value.IsLoaded
        && WorkspaceState.Value.List.Count(item => item.IsCreatedByCurrentUser && item.IsDefault == false)
            < GlobalConstants.MaxActiveCreatedWorkspaces;

    private bool IsTeamInvitationAvailable => AuthState.Value.Workspace?.Mode == WorkspaceMode.Team
        && AuthState.Value.IsRoleAdmin
        && SecurityManager.HasPermission(WorkspacePermission.UpdateWorkspaceMembers);

    private bool IsWorkspaceSettingsAvailable => SecurityManager.HasPermission(WorkspacePermission.UpdateWorkspace);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        WorkspacePermissionsState.StateChanged += OnWorkspacePermissionsStateChanged;
    }

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

    private async Task OpenInviteTeamModal()
    {
        await ModalDialogService.ShowAddWorkspaceMemberModal();
    }

    private async Task OpenAddWorkspaceModal()
    {
        await ModalDialogService.ShowAddWorkspaceModal();
    }

    private async Task OpenSupportModal()
    {
        await ModalDialogService.ShowSupportModal();
    }

    public void Dispose()
    {
        WorkspacePermissionsState.StateChanged -= OnWorkspacePermissionsStateChanged;
    }

    private void OnWorkspacePermissionsStateChanged(object? sender, EventArgs args)
    {
        _ = InvokeAsync(StateHasChanged);
    }

}
