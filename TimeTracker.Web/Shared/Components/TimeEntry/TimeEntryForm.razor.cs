using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Core.Components;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Shared.Components.TimeEntry;

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
}
