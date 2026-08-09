using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class MyTimeEntriesListBlock
{
    private abstract record ListItem;

    private sealed record DateHeaderListItem(
        DateTime Date,
        TimeSpan TotalDuration,
        IReadOnlyList<ClientDurationItem> ClientDurations
    ) : ListItem;

    private sealed record TimeEntryListItem(TimeEntryDto Entry) : ListItem;

    private record ClientDurationItem(string ClientName, TimeSpan Duration);

    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private bool _isLoading => _state.Value.IsListLoading;
    private string NoClientLabel => DashboardLocalizer["NoClient"].Value;
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

    private ICollection<ListItem> GetListItems()
    {
        return _state.Value.ListToShow
            .GroupBy(item => item.StartTimeOffset.Date)
            .SelectMany(group =>
            {
                var entries = group.ToList();
                var totalDuration = TimeSpan.FromTicks(entries.Sum(item => item.Duration.Ticks));
                var dateHeader = new DateHeaderListItem(
                    group.Key,
                    totalDuration,
                    GetClientDurationItems(entries)
                );

                return entries
                    .Select(entry => (ListItem)new TimeEntryListItem(entry))
                    .Prepend(dateHeader);
            })
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
}
