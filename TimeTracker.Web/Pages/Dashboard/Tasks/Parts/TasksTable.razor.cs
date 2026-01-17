using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;


namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksTable
{
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }

    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private long? _taskListId;
    
    [CascadingParameter(Name = "TaskListId")]
    public long? TaskListId
    {
        get => _selectedTasksListId;
        set
        {
            _selectedTasksListId = value;
        }
    }

    private long? _selectedTasksListId = null;
}
