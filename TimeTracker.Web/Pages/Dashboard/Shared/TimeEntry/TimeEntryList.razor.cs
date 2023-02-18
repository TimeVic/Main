using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Services.Format;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;

public partial class TimeEntryList
{
    [Parameter]
    public bool IsFilteredList { get; set; } = false;
    
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
    
    private ICollection<TimeEntryDto> _list => IsFilteredList ? _state.Value.FilteredList : _state.Value.ListToShow;
    
    private IEnumerable<IGrouping<DateTime, TimeEntryDto>> _groupedList => _list.GroupBy(item => item.Date);
    
    private int _totalCounter => IsFilteredList ? _state.Value.FilteredTotalCount : _state.Value.TotalCount;

    private void OnPageChanged(PagerEventArgs pagingEvent)
    {
        if (IsFilteredList)
        {
            Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction(pagingEvent.Skip));    
        }
        else
        {
            Dispatcher.Dispatch(new LoadListAction(pagingEvent.Skip));
        }
    }
}
