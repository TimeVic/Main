using TimeTracker.Web.Core.Components;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry;

public partial class TimeEntryPage: BaseReactiveComponent
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.SetSelectedPageAction(1));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.TimeEntry.LoadListAction());
    }
}
