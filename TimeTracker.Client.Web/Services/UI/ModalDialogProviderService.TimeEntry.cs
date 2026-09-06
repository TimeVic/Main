using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.TimeEntry.Manage;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowEditTimeEntryModal(TimeEntryDto timeEntry, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<EditTimeEntryModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(EditTimeEntryModal.TimeEntry)] = timeEntry
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
}
