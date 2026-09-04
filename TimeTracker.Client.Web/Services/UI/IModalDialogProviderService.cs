using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Web.Ui.Components.Core.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public interface IModalDialogProviderService
{
    Task<AppModalResult> ShowEditTaskModal(TaskDto task, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddWorkspaceMemberModal(Action<AppModalResult>? onClose = null);

    Task<bool> ShowConfirmationAsync(
        string message,
        string? title = null,
        string? confirmText = null,
        string? cancelText = null,
        AppConfirmationType type = AppConfirmationType.Alert
    );
}
