using TimeTracker.Web.Core.Components;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry;

public partial class TimeEntryPage: BaseComponent
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction(0));
    }
}
