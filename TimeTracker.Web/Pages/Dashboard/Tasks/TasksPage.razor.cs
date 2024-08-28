using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks;

public partial class TasksPage
{
    [Parameter]
    public long? TaskListId { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    public TaskListDto? _selectedTaskList
    {
        get => _tasksListState.Value.SelectedTaskList;
    }
}
