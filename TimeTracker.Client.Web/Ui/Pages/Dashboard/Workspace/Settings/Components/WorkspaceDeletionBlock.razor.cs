using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components;

public partial class WorkspaceDeletionBlock
{
    private bool _isConfirmationOpened;
    private string _confirmationName = string.Empty;

    private WorkspaceDto? Workspace => AuthState.Value.Workspace;

    private bool IsDeletionAvailable => Workspace?.CurrentUserAccess == MembershipAccessType.Owner
        && Workspace.IsDefault == false;

    private Task OpenConfirmation()
    {
        _isConfirmationOpened = true;
        return Task.CompletedTask;
    }

    private Task CloseConfirmation()
    {
        _isConfirmationOpened = false;
        _confirmationName = string.Empty;
        return Task.CompletedTask;
    }

    private async Task DeleteWorkspaceAsync()
    {
        if (Workspace == null)
        {
            return;
        }

        if (!string.Equals(Workspace.Name, _confirmationName.Trim(), StringComparison.Ordinal))
        {
            ToastService.ShowError(DashboardLocalizer["WorkspaceDeletionBlock_ConfirmationError"].Value);
            return;
        }

        try
        {
            await ApiService.WorkspaceDeleteAsync(new DeleteRequest
            {
                WorkspaceId = Workspace.Id,
                ConfirmationName = _confirmationName
            });
            ToastService.ShowSuccess(DashboardLocalizer["WorkspaceDeletionBlock_Deleted"].Value);
            Dispatcher.Dispatch(new LoadCurrentUserAction());
            Dispatcher.Dispatch(new LoadListAction(true));

            var defaultWorkspaceId = AuthState.Value.User.DefaultWorkspace?.Id;
            if (defaultWorkspaceId.HasValue)
            {
                NavigationManager.NavigateTo(UrlService.GetDashboardUrl(string.Empty, defaultWorkspaceId.Value), true);
            }
        }
        catch (Exception exception)
        {
            ToastService.ShowError(exception.Message);
        }
        finally
        {
            await CloseConfirmation();
        }
    }
}
