using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.TasksList;
using TimeTracker.Client.Web.Services.LastOpenedEntity;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Components;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks;

public partial class TasksPage: IDisposable
{
    private const int DefaultTaskListsWidth = 320;
    private const int MinTaskListsWidth = DefaultTaskListsWidth - 80;
    private const int MaxTaskListsWidth = DefaultTaskListsWidth + 240;

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

    private ClientDto? _selectedClient;
    private ProjectDto? _selectedProject;

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
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction(TaskListId));
            await SaveLastOpenedTaskListAsync(TaskListId.Value);
            return;
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

    private Task OnTaskListsContextChanged(TaskListsNavigationContext context)
    {
        if (_selectedClient?.Id == context.Client?.Id && _selectedProject?.Id == context.Project?.Id)
        {
            return Task.CompletedTask;
        }

        _selectedClient = context.Client;
        _selectedProject = context.Project;
        return InvokeAsync(StateHasChanged);
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
