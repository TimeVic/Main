using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.GoalsTracker;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker;

public partial class GoalsTrackerPage
{
    [Inject]
    private IState<GoalsTrackerState> _state { get; set; }
    
    private DateTime _selectedDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        LoadTracker();
    }

    private void OnMonthChanged(bool isForward = true)
    {
        _selectedDate = _selectedDate.AddMonths(isForward ? 1 : -1);
        LoadTracker();
    }

    private void LoadTracker()
    {
        Dispatcher.Dispatch(new LoadTrackerAction(_selectedDate));
    }
}
