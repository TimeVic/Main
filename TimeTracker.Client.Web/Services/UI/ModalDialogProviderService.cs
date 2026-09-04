using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Web.Ui.Components.Core.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals.EditTask;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Members.Parts;

namespace TimeTracker.Client.Web.Services.UI;

public class ModalDialogProviderService : IModalDialogProviderService
{
    private readonly IAppModalDialogService _appModalDialogService;

    public ModalDialogProviderService(IAppModalDialogService appModalDialogService)
    {
        _appModalDialogService = appModalDialogService;
    }

    public Task<AppModalResult> ShowEditTaskModal(TaskDto task, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<EditTaskModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(EditTaskModal.Task)] = task
            },
            options: new AppModalOptions
            {
                Size = AppModalSize.ExtraLarge,
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

    public Task<bool> ShowConfirmationAsync(
        string message,
        string? title = null,
        string? confirmText = null,
        string? cancelText = null,
        AppConfirmationType type = AppConfirmationType.Alert
    )
    {
        return _appModalDialogService.ShowConfirmationAsync(
            message,
            title,
            confirmText,
            cancelText,
            type
        );
    }
}
