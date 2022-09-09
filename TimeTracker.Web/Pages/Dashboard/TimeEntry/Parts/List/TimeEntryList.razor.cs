using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry.Parts.List;

public partial class TimeEntryList
{
    [Inject] 
    private IState<TimeEntryState> _state { get; set; }
}
