using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Services.Security;
using TimeTracker.Client.Core.Store.Client;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Components;

public record TaskListsNavigationContext(ClientDto? Client, ProjectDto? Project);

public partial class TasksListBlock : IDisposable
{
    [CascadingParameter(Name = "TaskListId")]
    public Guid? TaskListId { get; set; }

    [Parameter]
    public bool IsEmbedded { get; set; }

    [Parameter]
    public EventCallback<TaskListsNavigationContext> OnContextChanged { get; set; }

    [Inject]
    public IState<ClientState> _clientState { get; set; } = null!;

    [Inject]
    public IState<ProjectState> _projectState { get; set; } = null!;

    [Inject]
    public IState<TasksListState> _tasksListState { get; set; } = null!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ISecurityManager SecurityManager { get; set; } = null!;

    private readonly HashSet<Guid> _expandedClientIds = [];
    private readonly HashSet<Guid> _expandedProjectIds = [];
    private string? _taskListSearch;
    private bool _isAddClientModalOpened;
    private bool _isAddProjectModalOpened;
    private bool _isAddTaskListModalOpened;
    private bool _isShowUpdateTaskListModal;
    private bool _isShowDeleteTaskListConfirmation;
    private ClientDto? _selectedClient;
    private ClientDto? _clientForNewProject;
    private ProjectDto? _selectedProject;
    private TaskListDto? _taskListToManage;

    private TaskListDto? _selectedTaskList => _tasksListState.Value.SelectedTaskList;

    private string ContainerClass => IsEmbedded
        ? "flex min-h-[720px] w-full flex-col bg-white"
        : "rounded-lg border border-slate-200 bg-white shadow-sm";

    private IEnumerable<ClientDto> _clients => _clientState.Value.List
        .OrderBy(client => client.Name);

    private bool IsSearching => !string.IsNullOrWhiteSpace(_taskListSearch);

    private bool IsCanCreateClient => SecurityManager.HasPermission(WorkspacePermission.CreateClient);

    private bool IsCanCreateProject => SecurityManager.HasPermission(WorkspacePermission.CreateProject);

    private IEnumerable<TaskListDto> _searchResults => _tasksListState.Value.List
        .Where(MatchesSearch)
        .OrderBy(taskList => taskList.Project.Client?.Name)
        .ThenBy(taskList => taskList.Project.Name)
        .ThenBy(taskList => taskList.Name);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _tasksListState.StateChanged += OnTasksListStateChanged;

        if (!_tasksListState.Value.IsLoaded || _tasksListState.Value.SelectedProjectId.HasValue)
        {
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.LoadListAction());
        }

        SetContextFromSelectedTaskList();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (TaskListId.HasValue && _tasksListState.Value.SelectedTaskListId != TaskListId)
        {
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetSelectedAction(TaskListId));
            SetContextFromSelectedTaskList();
        }
    }

    public void Dispose()
    {
        _tasksListState.StateChanged -= OnTasksListStateChanged;
    }

    private IEnumerable<ProjectDto> GetClientProjects(ClientDto client) => _projectState.Value.List
        .Where(project => project.Client?.Id == client.Id)
        .OrderBy(project => project.Name);

    private IEnumerable<TaskListDto> GetProjectTaskLists(ProjectDto project) => _tasksListState.Value.List
        .Where(taskList => taskList.Project.Id == project.Id)
        .OrderBy(taskList => taskList.Name);

    private bool IsClientExpanded(ClientDto client) => _expandedClientIds.Contains(client.Id);

    private bool IsProjectExpanded(ProjectDto project) => _expandedProjectIds.Contains(project.Id);

    private async Task OnClientSelected(ClientDto client)
    {
        ToggleExpanded(_expandedClientIds, client.Id);
        _selectedClient = client;
        _selectedProject = null;
        await NotifyContextChanged();
    }

    private async Task OnProjectSelected(ProjectDto project)
    {
        _expandedClientIds.Add(project.Client!.Id);
        ToggleExpanded(_expandedProjectIds, project.Id);
        _selectedClient = project.Client;
        _selectedProject = project;
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetSelectedAction(project));
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetSelectedAction(null));
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction(null));
        await NotifyContextChanged();
        NavigationManager.NavigateTo(UrlService.GetDashboardUrl("tasks"));
    }

    private async Task OnTasksListSelected(TaskListDto taskList)
    {
        _expandedClientIds.Add(taskList.Project.Client!.Id);
        _expandedProjectIds.Add(taskList.Project.Id);
        _selectedClient = taskList.Project.Client;
        _selectedProject = taskList.Project;
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetSelectedAction(taskList.Project));
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetSelectedAction(taskList.Id));
        await NotifyContextChanged();
        NavigationManager.NavigateTo(UrlService.GetDashboardUrl($"tasks/{taskList.Id}"));
    }

    private Task OpenAddTaskListModal(ProjectDto project)
    {
        _selectedClient = project.Client;
        _selectedProject = project;
        _expandedClientIds.Add(project.Client!.Id);
        _expandedProjectIds.Add(project.Id);
        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetSelectedAction(project));
        _isAddTaskListModalOpened = true;
        return NotifyContextChanged();
    }

    private Task OpenAddClientModal()
    {
        if (!IsCanCreateClient)
        {
            return Task.CompletedTask;
        }

        _isAddClientModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OpenAddProjectModal(ClientDto client)
    {
        if (!IsCanCreateProject)
        {
            return Task.CompletedTask;
        }

        _clientForNewProject = client;
        _isAddProjectModalOpened = true;
        return Task.CompletedTask;
    }

    private Task OnAddProjectModalOpenedChanged(bool isOpened)
    {
        _isAddProjectModalOpened = isOpened;
        if (!isOpened)
        {
            _clientForNewProject = null;
        }

        return Task.CompletedTask;
    }

    private string GetClientClass(ClientDto client) => _selectedClient?.Id == client.Id && _selectedProject == null
        ? "flex w-full items-center gap-2 rounded-md bg-slate-100/80 px-2 py-2 text-left text-slate-600"
        : "flex w-full items-center gap-2 rounded-md px-2 py-2 text-left text-slate-500 transition hover:bg-slate-50 hover:text-slate-700";

    private string GetProjectClass(ProjectDto project) => _selectedProject?.Id == project.Id && _selectedTaskList == null
        ? "flex min-w-0 flex-1 items-center gap-2 rounded-md bg-slate-100 px-2 py-1.5 text-left text-slate-900"
        : "flex min-w-0 flex-1 items-center gap-2 rounded-md bg-slate-50/80 px-2 py-1.5 text-left text-slate-700 transition hover:bg-slate-100";

    private string GetTaskListClass(TaskListDto taskList) => _selectedTaskList?.Id == taskList.Id
        ? "flex min-w-0 flex-1 items-center justify-between rounded-md border-l-2 border-blue-500 bg-blue-50 px-2 py-1.5 text-left"
        : "flex min-w-0 flex-1 items-center justify-between rounded-md border-l-2 border-transparent px-2 py-1.5 text-left transition hover:bg-slate-50";

    private string GetTaskListContext(TaskListDto taskList) =>
        $"{taskList.Project.Client?.Name ?? DashboardLocalizer["NoClient"].Value} · {taskList.Project.Name}";

    private string GetTasksCountText(TaskListDto taskList)
    {
        var tasksCount = taskList is TaskListForListDto taskListWithCounter
            ? taskListWithCounter.TasksCount
            : 0;
        var key = tasksCount == 1 ? "TasksListBlock_TaskCount" : "TasksListBlock_TasksCount";
        return string.Format(DashboardLocalizer[key].Value, tasksCount);
    }

    private string GetProjectTaskListsCountText(ProjectDto project)
    {
        var count = GetProjectTaskLists(project).Count();
        var key = count == 1
            ? "TasksListBlock_TaskListCount"
            : "TasksListBlock_TaskListsCount";
        return string.Format(DashboardLocalizer[key].Value, count);
    }

    private bool MatchesSearch(TaskListDto taskList)
    {
        var search = _taskListSearch!.Trim();
        return taskList.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || taskList.Project.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || taskList.Project.Client?.Name.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
    }

    private Task OnTasksListAdded(TaskListDto? taskList) => taskList == null
        ? Task.CompletedTask
        : OnTasksListSelected(taskList);

    private Task OpenEditTaskList(TaskListDto taskList)
    {
        _taskListToManage = taskList;
        _isShowUpdateTaskListModal = true;
        return Task.CompletedTask;
    }

    private Task OpenDeleteTaskListConfirmation(TaskListDto taskList)
    {
        _taskListToManage = taskList;
        _isShowDeleteTaskListConfirmation = true;
        return Task.CompletedTask;
    }

    private Task OnDeleteTaskList()
    {
        if (_taskListToManage != null)
        {
            Dispatcher.Dispatch(new ArchiveTaskListAction(_taskListToManage));
        }

        _isShowDeleteTaskListConfirmation = false;
        _taskListToManage = null;
        return Task.CompletedTask;
    }

    private void OnTasksListStateChanged(object? sender, EventArgs args)
    {
        SetContextFromSelectedTaskList();
        _ = InvokeAsync(StateHasChanged);
    }

    private void SetContextFromSelectedTaskList()
    {
        if (_selectedTaskList?.Project == null)
        {
            return;
        }

        var isContextChanged = _selectedClient?.Id != _selectedTaskList.Project.Client?.Id
            || _selectedProject?.Id != _selectedTaskList.Project.Id;
        _selectedClient = _selectedTaskList.Project.Client;
        _selectedProject = _selectedTaskList.Project;
        if (_selectedClient != null)
        {
            _expandedClientIds.Add(_selectedClient.Id);
        }

        _expandedProjectIds.Add(_selectedProject.Id);
        if (isContextChanged)
        {
            _ = InvokeAsync(NotifyContextChanged);
        }
    }

    private Task NotifyContextChanged() => OnContextChanged.InvokeAsync(
        new TaskListsNavigationContext(_selectedClient, _selectedProject)
    );

    private static void ToggleExpanded(ISet<Guid> expandedIds, Guid id)
    {
        if (!expandedIds.Add(id))
        {
            expandedIds.Remove(id);
        }
    }
}
