using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts.TimeEntryForm;

public partial class TimeEntryForm
{
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }

    private void StartTimeEntry()
    {
        Dispatcher.Dispatch(new StartTimeEntryAction());
    }

    private void StopTimeEntry()
    {
        Dispatcher.Dispatch(new StopActiveTimeEntryAction());
    }
    
    private async Task OnChangeStartTime(TimeSpan startTime)
    {
        
        await Task.CompletedTask;
    }

    private async Task OnChangeEndTime(TimeSpan time)
    {
        await Task.CompletedTask;
    }

    private async Task OnChangeDescription(string value)
    {
        await Task.CompletedTask;
    }
}
