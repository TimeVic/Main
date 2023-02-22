using Microsoft.AspNetCore.Components;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TasksPage
{
    [Parameter]
    public long ClientId
    {
        get => _clientId;
        set
        {
            _clientId = value;
            OnTasksListSelected(null);
        }
    }
    
    [Parameter]
    public long? TaskListId
    {
        get => _taskListId;
        set
        {
            _taskListId = value;
            OnTasksListSelected(_taskListId);
        }
    }

    private long _clientId = 0;
    private long? _taskListId = null;

    private void OnTasksListSelected(long? testsListId)
    {
        Dispatcher.Dispatch(new Store.TasksList.SetSelected(testsListId));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction(0));
    }
}
