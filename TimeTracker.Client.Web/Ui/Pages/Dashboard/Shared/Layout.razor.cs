using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Client.Core.Core.Extensions;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Services.UI;
using TimeTracker.Client.Web.Constants;
using TimeTracker.Client.Web.Services.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared;

public partial class Layout: IDisposable
{
    [Inject]
    private WorkspaceInitializationService _workspaceInitializationService { get; set; } = null!;
    
    [Inject]
    private NavigationManager _navigationManager { get; set; } = null!;

    [Inject]
    private UrlService _urlService { get; set; } = null!;

    [Inject]
    private ISecurityManager _securityManager { get; set; } = null!;

    private IDisposable? _locationChangingRegistration;
    
    protected override async Task OnInitializedAsync()
    {
        IsRedirectIfNotLoggedIn = true;
        AuthState.StateChanged += OnAuthStateChanged;
        _locationChangingRegistration = _navigationManager.RegisterLocationChangingHandler(OnLocationChangingAsync);
        await base.OnInitializedAsync();
        CheckWorkspaceModeRedirect();
    }

    public new void Dispose()
    {
        AuthState.StateChanged -= OnAuthStateChanged;
        _locationChangingRegistration?.Dispose();
        base.Dispose();
    }

    private void OnAuthStateChanged(object? sender, EventArgs e)
    {
        CheckWorkspaceModeRedirect();
    }

    private void CheckWorkspaceModeRedirect(string? targetLocation = null)
    {
        var workspace = AuthState.Value.Workspace;
        if (workspace != null
            && !workspace.Mode.HasValue
            && _securityManager.HasPermission(WorkspacePermission.UpdateWorkspace))
        {
            var currentPath = (targetLocation ?? _navigationManager.GetPath()).ToLower();
            if (!currentPath.Contains("choose-mode"))
            {
                _navigationManager.NavigateTo(
                    _urlService.GetDashboardUrl("workspace/choose-mode", workspace.Id),
                    replace: true
                );
            }
        }
        else if (workspace != null && !_securityManager.HasPermission(WorkspacePermission.ReadMemberPayment))
        {
            var currentPath = (targetLocation ?? _navigationManager.GetPath()).ToLower();
            if (currentPath.Contains("member-payments"))
            {
                _navigationManager.NavigateTo(
                    _urlService.GetDashboardUrl(workspaceId: workspace.Id),
                    replace: true
                );
            }
        }
    }

    private async ValueTask OnLocationChangingAsync(LocationChangingContext context)
    {
        var workspaceId = _urlService.GetWorkspaceIdFromDashboardUrl(context.TargetLocation);
        if (!workspaceId.HasValue || workspaceId == AuthState.Value.Workspace?.Id)
        {
            CheckWorkspaceModeRedirect(context.TargetLocation);
            return;
        }

        if (await _workspaceInitializationService.EnsureWorkspaceAsync(workspaceId))
        {
            _workspaceInitializationService.Init(isReload: true);
            await _workspaceInitializationService.AfterInit(isReload: true);
            CheckWorkspaceModeRedirect(context.TargetLocation);
            return;
        }

        context.PreventNavigation();
        _navigationManager.NavigateTo(SiteUrl.Error403, replace: true);
    }
}
