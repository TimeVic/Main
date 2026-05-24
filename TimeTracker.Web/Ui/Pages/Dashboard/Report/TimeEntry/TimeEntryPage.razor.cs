using TimeTracker.Client.Core.Store.TimeEntry;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Report.TimeEntry;

public partial class TimeEntryPage
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new SetFilteredSelectedPageAction(1));
        Dispatcher.Dispatch(new LoadTimeEntryFilteredListAction());
    }
}
