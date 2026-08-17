using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Core.Services.Security;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace;

public partial class ChooseWorkspaceModePage
{
    private bool _isLoading = false;
    private WorkspaceMode? _selectedMode = null;

    [Inject]
    private ISecurityManager SecurityManager { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        CheckRedirect();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        CheckRedirect();
    }

    private void CheckRedirect()
    {
        var currentWorkspace = AuthState.Value.Workspace;
        if (currentWorkspace == null || currentWorkspace.Id != WorkspaceId)
        {
            return;
        }

        if (currentWorkspace.Mode.HasValue)
        {
            NavigationManager.NavigateTo(UrlService.GetDashboardUrl(workspaceId: WorkspaceId), replace: true);
        }
    }

    private async Task SelectModeAsync(WorkspaceMode mode)
    {
        if (_isLoading)
            return;

        if (!SecurityManager.HasPermission(WorkspacePermission.UpdateWorkspace)
            || AuthState.Value.Workspace?.Id != WorkspaceId)
            return;

        _isLoading = true;
        _selectedMode = mode;
        StateHasChanged();

        try
        {
            var updatedWorkspace = await ApiService.WorkspaceSetModeAsync(mode);
            if (updatedWorkspace != null)
            {
                Dispatcher.Dispatch(new SetWorkspaceAction(updatedWorkspace));
                NavigationManager.NavigateTo(UrlService.GetDashboardUrl(workspaceId: WorkspaceId), replace: true);
            }
        }
        catch
        {
            // Ignored
        }
        finally
        {
            _isLoading = false;
            _selectedMode = null;
            StateHasChanged();
        }
    }
}
