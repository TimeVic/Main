using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<bool> ShowConfirmationAsync(
        string message,
        string? title = null,
        string? confirmText = null,
        string? cancelText = null,
        AppConfirmationType type = AppConfirmationType.Alert
    );

    Task<bool> ShowConfirmationAsync(AppConfirmationOptions options);
}
