using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.TasksList;
using TimeTracker.Client.Web.Services.LastOpenedEntity;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks;

public partial class TasksPage: IDisposable
{
    [Parameter]
    public Guid? TaskListId { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }

    [Inject]
    public ILastOpenedEntityService _lastOpenedEntityService { get; set; }
    
    public TaskListDto? _selectedTaskList
    {
        get => _tasksListState.Value.SelectedTaskList;
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _tasksListState.StateChanged += OnTasksListStateChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (TaskListId.HasValue)
        {
            Dispatcher.Dispatch(new SetSelectedAction(TaskListId));
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction());
            await SaveLastOpenedTaskListAsync(TaskListId.Value);
            return;
        }

        await NavigateToLastOpenedTaskListAsync();
    }

    public void Dispose()
    {
        _tasksListState.StateChanged -= OnTasksListStateChanged;
    }

    private void OnTasksListStateChanged(object? sender, EventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    private async Task NavigateToLastOpenedTaskListAsync()
    {
        var workspaceId = GetCurrentWorkspaceId();
        if (!workspaceId.HasValue)
        {
            return;
        }

        var taskListId = await _lastOpenedEntityService.GetLastOpenedIdAsync(
            workspaceId.Value,
            LastOpenedEntityType.TaskList
        );
        if (taskListId.HasValue && TaskListId == null)
        {
            NavigationManager.NavigateTo(
                UrlService.GetDashboardUrl($"tasks/{taskListId.Value}", workspaceId.Value),
                replace: true
            );
        }
    }

    private Task SaveLastOpenedTaskListAsync(Guid taskListId)
    {
        var workspaceId = GetCurrentWorkspaceId();
        return workspaceId.HasValue
            ? _lastOpenedEntityService.SetLastOpenedIdAsync(
                workspaceId.Value,
                LastOpenedEntityType.TaskList,
                taskListId
            )
            : Task.CompletedTask;
    }

    private Guid? GetCurrentWorkspaceId()
    {
        return WorkspaceId != Guid.Empty
            ? WorkspaceId
            : AuthState.Value.Workspace?.Id;
    }
}
