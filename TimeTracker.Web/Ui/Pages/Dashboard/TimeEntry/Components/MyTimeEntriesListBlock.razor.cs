using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.TimeEntry.Components;

public partial class MyTimeEntriesListBlock
{
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    [Inject]
    private ITimeParsingService _timeParsingService { get; set; }
    
    private IEnumerable<IGrouping<DateOnly, TimeEntryDto>> _groupedList => _state.Value.ListToShow.GroupBy(item => item.Date);
    private bool _isLoading => _state.Value.IsListLoading;
    
    private void OnPaginated(int selectedPageIndex)
    {
        var selectedPage = selectedPageIndex + 1;
        Dispatcher.Dispatch(new SetSelectedPageAction(selectedPage));
        Dispatcher.Dispatch(new LoadListAction());
    }
}
