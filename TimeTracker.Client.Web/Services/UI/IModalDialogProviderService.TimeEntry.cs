using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowEditTimeEntryModal(TimeEntryDto timeEntry, Action<AppModalResult>? onClose = null);
}
