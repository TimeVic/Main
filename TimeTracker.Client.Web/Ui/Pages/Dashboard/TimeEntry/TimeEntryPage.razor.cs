using TimeTracker.Client.Core.Core.Components;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.TimeEntry;

public partial class TimeEntryPage: BaseReactiveComponent, IDisposable
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.SetIsTimeEntryListVisibleAction(true));
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.SetSelectedPageAction(1));
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.LoadListAction());
    }

    public void Dispose()
    {
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.SetIsTimeEntryListVisibleAction(false));
    }
}
