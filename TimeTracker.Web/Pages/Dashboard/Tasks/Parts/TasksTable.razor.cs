using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksTable
{
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }

    [CascadingParameter(Name = "TaskListId")]
    public long? TaskListId
    {
        get => _selectedTasksListId;
        set
        {
            _selectedTasksListId = value;
            UpdateTaskList();
        }
    }

    private long? _selectedTasksListId = null;
    private TaskListDto? _selectedTasksList = null;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _tasksListState.StateChanged += (sender, args) =>
        {
            UpdateTaskList();
        };
    }

    private void UpdateTaskList()
    {
        _selectedTasksList = _tasksListState.Value.List.FirstOrDefault(item => item.Id == _selectedTasksListId);
        StateHasChanged();
    }
}
