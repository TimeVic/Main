using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService : IModalDialogProviderService
{
    private readonly IAppModalDialogService _appModalDialogService;

    public ModalDialogProviderService(IAppModalDialogService appModalDialogService)
    {
        _appModalDialogService = appModalDialogService;
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

    public Task<bool> ShowConfirmationAsync(AppConfirmationOptions options)
    {
        return _appModalDialogService.ShowConfirmationAsync(options);
    }
}
