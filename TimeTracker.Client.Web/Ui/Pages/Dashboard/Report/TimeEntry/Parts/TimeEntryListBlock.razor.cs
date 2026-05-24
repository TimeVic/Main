using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Report.TimeEntry.Parts;

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

    private void OnConfirmDelete()
    {
        if (_timeEntryToDelete == null)
            return;

        var entryId = _timeEntryToDelete.Id;
        _timeEntryToDelete = null;

        Dispatcher.Dispatch(new DeleteTimeEntryAction(entryId));
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
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
