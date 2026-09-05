using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Workspace;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Modals;

public partial class DeleteWorkspaceModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public WorkspaceDto? Workspace { get; set; }

    private string _confirmationName = string.Empty;
    private bool _isLoading;

    private async Task Cancel()
    {
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Cancel());
        }
    }

    private async Task Submit()
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

        _isLoading = true;
        try
        {
            await ApiService.WorkspaceDeleteAsync(new DeleteRequest
            {
                WorkspaceId = Workspace.Id,
                ConfirmationName = _confirmationName
            });
            ToastService.ShowSuccess(DashboardLocalizer["WorkspaceDeletionBlock_Deleted"].Value);
            var user = await ApiService.UserGetCurrentAsync();
            var defaultWorkspace = user?.DefaultWorkspace;

            if (user != null)
            {
                Dispatcher.Dispatch(new UpdateUserAction(user));
            }

            if (defaultWorkspace != null)
            {
                Dispatcher.Dispatch(new SetWorkspaceAction(defaultWorkspace));
            }

            Dispatcher.Dispatch(new LoadListAction(true));

            if (ModalInstance != null)
            {
                await ModalInstance.Close(AppModalResult.Ok());
            }

            if (defaultWorkspace != null)
            {
                NavigationManager.NavigateTo(UrlService.GetDashboardUrl(string.Empty, defaultWorkspace.Id), true);
            }
        }
        catch (Exception exception)
        {
            ToastService.ShowError(exception.Message);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
