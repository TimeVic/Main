using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.TimeEntry.Parts;

public partial class TimeEntryListBlock
{
    [Inject]
    private IState<TimeEntryState> _state { get; set; } = null!;

    [Inject]
    private ITimeParsingService _timeParsingService { get; set; } = null!;

    private TimeEntryDto? _timeEntryToEdit;
    private TimeEntryDto? _timeEntryToDelete;

    private void OnCloseEditModal()
    {
        _timeEntryToEdit = null;
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
    }

    private async Task OnConfirmDelete()
    {
        if (_timeEntryToDelete == null)
            return;

        var entryId = _timeEntryToDelete.Id;
        _timeEntryToDelete = null;

        try
        {
            await ApiService.TimeEntryDeleteAsync(entryId);
            Dispatcher.Dispatch(new DeleteTimeEntryFromListAction(entryId));
            Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
            ToastService.ShowInfo("Time entry deleted!");
        }
        catch (Exception)
        {
            ToastService.ShowError("Failed to delete time entry.");
        }
    }

    private void OnPageChanged(int page)
    {
        Dispatcher.Dispatch(new SetFilteredSelectedPageAction(page));
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
    }

    private static string GetDescriptionOrTask(TimeEntryDto entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.Description))
            return entry.Description.TruncateAndAddDots(120);

        if (!string.IsNullOrWhiteSpace(entry.Task?.Title))
            return entry.Task.Title.TruncateAndAddDots(120);

        return string.Empty;
    }
}
