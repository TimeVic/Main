using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TasksPage
{
    [Parameter]
    public long? ClientId
    {
        get => _clientId;
        set
        {
            _clientId = value;
            OnTestsListSelected(null);
        }
    }

    private long? _clientId = null;

    private void OnTestsListSelected(long? testsListId)
    {
        Dispatcher.Dispatch(new Store.TasksList.SetSelected(testsListId));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction(0));
    }
}
