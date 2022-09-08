using TimeTracker.Web.Core.Components;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.TimeEntry;

namespace TimeTracker.Web.Pages.Dashboard.TimeEntry;

public partial class TimeEntryPage: BaseComponent
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadTimeEntryListAction(1));
    }
}
