using TimeTracker.Client.Core.Core.Components;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.TimeEntry;

public partial class TimeEntryPage: BaseReactiveComponent
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.SetSelectedPageAction(1));
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TimeEntry.LoadListAction());
    }
}
