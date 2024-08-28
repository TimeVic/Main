using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public async Task ShowTimeEntryEditModal(TimeEntryDto timeEntry)
    {
        var parameters = new DialogParameters
        {
            Width = "500px",
            PrimaryAction = ""
        };
        await _dialogService.ShowDialogAsync<TimeEntryEditModal>(
            new TimeEntryEditModal.Parameters
            {
                TimeEntry = timeEntry,
            },
            parameters
        );
    }
}
