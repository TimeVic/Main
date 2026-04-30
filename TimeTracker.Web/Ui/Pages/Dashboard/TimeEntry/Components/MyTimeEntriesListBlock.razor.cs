using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class MyTimeEntriesListBlock
{
    private const string NoClientLabel = "No client";

    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private IEnumerable<IGrouping<DateTime, TimeEntryDto>> _groupedList => _state.Value.ListToShow.GroupBy(item => item.StartTimeOffset.Date);
    private bool _isLoading => _state.Value.IsListLoading;
    private TimeEntryDto? _timeEntryToEdit { get; set; }
    private TimeEntryDto? _timeEntryToDelete { get; set; }
    private TaskDto? _taskToEdit { get; set; }

    private IReadOnlyList<ClientDurationItem> GetClientDurationItems(IEnumerable<TimeEntryDto> entries)
    {
        return entries
            .GroupBy(entry => new
            {
                ClientId = entry.Project?.Client?.Id,
                ClientName = string.IsNullOrWhiteSpace(entry.Project?.Client?.Name)
                    ? NoClientLabel
                    : entry.Project.Client.Name
            })
            .Select(group => new ClientDurationItem(
                group.Key.ClientName,
                TimeSpan.FromTicks(group.Sum(entry => entry.Duration.Ticks))
            ))
            .OrderBy(item => item.ClientName == NoClientLabel)
            .ThenBy(item => item.ClientName)
            .ToList();
    }
    
    private void OnPaginated(int selectedPageIndex)
    {
        var selectedPage = selectedPageIndex + 1;
        Dispatcher.Dispatch(new SetSelectedPageAction(selectedPage));
        Dispatcher.Dispatch(new LoadListAction());
    }

    private void OnEditTimeEntry(TimeEntryDto entry)
    {
        _timeEntryToEdit = entry;
    }

    private void OnCloseEditTimeEntryModal()
    {
        _timeEntryToEdit = null;
    }

    private void OnCloneTimeEntry(TimeEntryDto timeEntry)
    {
        Dispatcher.Dispatch(  
            new StartTimeEntryAction(  
                IsBillable: timeEntry.IsBillable,  
                Project: timeEntry.Project,  
                Description: timeEntry.Description,
                HourlyRate: timeEntry.HourlyRate,  
                InternalTask: timeEntry.Task  
            )  
        );
    }

    private Task OnConfirmDeleteTimeEntry()
    {
        if (_timeEntryToDelete != null)
        {
            Dispatcher.Dispatch(new DeleteTimeEntryAction(_timeEntryToDelete.Id));
            _timeEntryToDelete = null;
        }
        return Task.CompletedTask;
    }

    private void OnPageChanged(int selectedPage)
    {
        Dispatcher.Dispatch(new SetSelectedPageAction(selectedPage));
        Dispatcher.Dispatch(new LoadListAction());
    }

    private record ClientDurationItem(string ClientName, TimeSpan Duration);
}
