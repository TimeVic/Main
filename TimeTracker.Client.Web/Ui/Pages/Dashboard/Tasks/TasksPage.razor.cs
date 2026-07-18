using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks;

public partial class TasksPage: IDisposable
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
        _tasksListState.StateChanged += OnTasksListStateChanged;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (TaskListId.HasValue)
        {
            Dispatcher.Dispatch(new SetSelectedAction(TaskListId));
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction());
        }
    }

    public void Dispose()
    {
        _tasksListState.StateChanged -= OnTasksListStateChanged;
    }

    private void OnTasksListStateChanged(object? sender, EventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }
}
