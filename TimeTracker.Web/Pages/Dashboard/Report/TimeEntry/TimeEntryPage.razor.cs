using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.Report.TimeEntry;

public partial class TimeEntryPage
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction(1));
    }
}
