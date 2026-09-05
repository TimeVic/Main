using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Modals;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members.Parts;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowAddWorkspaceModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<AddWorkspaceModal>(
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowAddWorkspaceMemberModal(Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<AddMemberModal>(
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowUpdateWorkspaceMemberModal(WorkspaceMemberDto member, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<UpdateMemberModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(UpdateMemberModal.Member)] = member
            },
            options: new AppModalOptions
            {
                Size = AppModalSize.Large,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowDeleteWorkspaceModal(WorkspaceDto workspace, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<DeleteWorkspaceModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(DeleteWorkspaceModal.Workspace)] = workspace
            },
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }
}
