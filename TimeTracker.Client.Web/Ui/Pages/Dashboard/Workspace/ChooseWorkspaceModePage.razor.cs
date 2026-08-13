using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;
using TimeTracker.Client.Core.Services.Http;
using TimeTracker.Client.Web.Constants;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace;

public partial class ChooseWorkspaceModePage
{
    private bool _isLoading = false;
    private WorkspaceMode? _selectedMode = null;

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
        if (currentWorkspace != null && currentWorkspace.Mode.HasValue)
        {
            NavigationManager.NavigateTo(SiteUrl.DashboardBase, replace: true);
        }
        else if (currentWorkspace != null && !currentWorkspace.IsFullAccess)
        {
            NavigationManager.NavigateTo(SiteUrl.DashboardBase, replace: true);
        }
    }

    private async Task SelectModeAsync(WorkspaceMode mode)
    {
        if (_isLoading)
            return;

        var targetWorkspaceId = WorkspaceId != Guid.Empty ? WorkspaceId : AuthState.Value.Workspace?.Id;
        if (!targetWorkspaceId.HasValue)
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
                NavigationManager.NavigateTo(SiteUrl.DashboardBase, replace: true);
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
