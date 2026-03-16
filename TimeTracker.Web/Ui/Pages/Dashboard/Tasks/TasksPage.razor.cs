using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks;

public partial class TasksPage
{
    [Parameter]
    public Guid? TaskListId { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }
    
    public TaskListDto? _selectedTaskList
    {
        get => _tasksListState.Value.SelectedTaskList;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (TaskListId.HasValue)
        {
            Dispatcher.Dispatch(new SetSelectedAction(TaskListId));
        }
        _tasksListState.StateChanged += (sender, args) =>
        {   
            StateHasChanged();
        };
    }
}
