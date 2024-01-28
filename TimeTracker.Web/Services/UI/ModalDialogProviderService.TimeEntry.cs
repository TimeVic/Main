using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowTimeEntryEditModal(TimeEntryDto timeEntry)
    {
        var parameters = new DialogParameters<TimeEntryEditModal>
        {
            { x => x.TimeEntry, timeEntry },
        };
        await _mudDialogService.ShowAsync<TimeEntryEditModal>("Edit Time Entry", parameters, new DialogOptions()
        {
            Position = DialogPosition.TopCenter,
            MaxWidth = MaxWidth.Medium,
            NoHeader = true,
            CloseButton = true
        });
    }
}
