using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.Common;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Core.Store.TasksList;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksListBlock: IDisposable
{
    [CascadingParameter(Name = "TaskListId")]
    public Guid? TaskListId
    {
        get => _taskListId;
        set => _taskListId = value;
    }

    [Inject]
    public IState<CommonState> _commonState { get; set; }
    
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }
    
    private Guid? _taskListId = null;
    private string? _taskListSearch = null;
    private bool _isAddTaskListModalOpened = false;
    private bool _isShowUpdateTaskListModal = false;
    private bool _isShowDeleteTaskListConfirmation = false;
    private ProjectDto? _selectedProject => _projectState.Value.Selected;

    public TaskListDto? _selectedTaskList
    {
        get => _tasksListState.Value.SelectedTaskList;
    }
    
    public IEnumerable<TaskListDto> _filteredTaskLists
    {
        get => _tasksListState.Value.List
            .Where(item => item.Project?.Id == _projectState.Value.Selected?.Id)
            .Where(item => string.IsNullOrWhiteSpace(_taskListSearch) || item.Name.Contains(_taskListSearch));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _projectState.StateChanged += OnProjectStateChanged;
        _tasksListState.StateChanged += OnTasksListStateChanged;
        SetDefaultTaskList();
        LoadSelectedProjectTaskLists();
        if (TaskListId == null)
        {
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction());
        }
    }

    public void Dispose()
    {
        _projectState.StateChanged -= OnProjectStateChanged;
        _tasksListState.StateChanged -= OnTasksListStateChanged;
    }
    
    private void OnTasksListSelected(Guid taskListId)
    {
        if (TaskListId == taskListId)
        {
            return;
        }

        NavigationManager.NavigateTo(UrlService.GetDashboardUrl($"tasks/{taskListId}"));
    }
    
    private void OnSelectedProject(ProjectDto? project)
    {
        if (project is null)
        {
            return;
        }

        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetSelectedAction(project));

        if (_tasksListState.Value.SelectedTaskList?.Project?.Id != project.Id)
        {
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetSelectedAction(null));
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction());
        }
    }
    
    private string GetTaskListClass(TaskListDto? taskList)
    {
        var selected = _selectedTaskList == taskList;
        return selected
            ? "flex w-full items-center justify-between rounded-2xl border border-blue-200 bg-blue-50 px-3 py-3 text-left"
            : "flex w-full items-center justify-between rounded-2xl border border-slate-200 px-3 py-3 text-left transition hover:border-slate-300 hover:bg-slate-50";
    }

    private string GetTasksCountText(TaskListDto taskList)
    {
        var tasksCount = taskList is TaskListForListDto taskListWithCounter
            ? taskListWithCounter.TasksCount
            : 0;

        var key = tasksCount == 1
            ? "TasksListBlock_TaskCount"
            : "TasksListBlock_TasksCount";

        return string.Format(DashboardLocalizer[key].Value, tasksCount);
    }
    
    private void OnTasksListStateChanged(object? sender, EventArgs e)
    {
        SetDefaultTaskList();
        InvokeAsync(StateHasChanged);
    }

    private void OnProjectStateChanged(object? sender, EventArgs e)
    {
        LoadSelectedProjectTaskLists();
        SetDefaultTaskList();
        InvokeAsync(StateHasChanged);
    }

    private void SetDefaultTaskList()
    {
        if (TaskListId.HasValue)
        {
            if (_tasksListState.Value.SelectedTaskListId != TaskListId)
            {
                Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetSelectedAction(TaskListId));
            }

            var routedTaskList = _tasksListState.Value.SelectedTaskList;
            if (routedTaskList?.Project != null
                && routedTaskList.Project.Id != _projectState.Value.Selected?.Id)
            {
                Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetSelectedAction(routedTaskList.Project));
            }

            return;
        }

        if (_projectState.Value.Selected == null && _tasksListState.Value.SelectedTaskList?.Project != null)
        {
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Project.SetSelectedAction(_tasksListState.Value.SelectedTaskList.Project));
            return;
        }

        var selectedProjectId = _projectState.Value.Selected?.Id;
        if (!selectedProjectId.HasValue)
        {
            return;
        }

        var selectedTaskList = _tasksListState.Value.SelectedTaskList;
        if (selectedTaskList?.Project?.Id == selectedProjectId.Value)
        {
            return;
        }

        var firstProjectTaskList = _tasksListState.Value.List.FirstOrDefault(item => item.Project.Id == selectedProjectId.Value);
        if (firstProjectTaskList != null)
        {
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetSelectedAction(firstProjectTaskList.Id));
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction());
            return;
        }

        if (_tasksListState.Value.SelectedTaskListId != null)
        {
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.SetSelectedAction(null));
            Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.Tasks.LoadListAction());
        }
    }
    
    private Task OnTasksListAdded(TaskListDto arg)
    {
        OnTasksListSelected(arg.Id);
        return Task.CompletedTask;
    }

    private Task OnEditTaskList()
    {
        if (_selectedTaskList == null)
        {
            return Task.CompletedTask;
        }

        _isShowUpdateTaskListModal = true;
        return Task.CompletedTask;
    }

    private Task OnDeleteTaskList()
    {
        if (_selectedTaskList == null)
        {
            return Task.CompletedTask;
        }

        Dispatcher.Dispatch(new ArchiveTaskListAction(_selectedTaskList));
        _isShowDeleteTaskListConfirmation = false;
        return Task.CompletedTask;
    }

    private void LoadSelectedProjectTaskLists()
    {
        var selectedProjectId = _projectState.Value.Selected?.Id;
        if (!selectedProjectId.HasValue)
        {
            if (_tasksListState.Value.List.Any() || _tasksListState.Value.SelectedProjectId.HasValue)
            {
                Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.LoadListAction(ProjectId: null));
            }

            return;
        }

        if (_tasksListState.Value.SelectedProjectId == selectedProjectId.Value
            && _tasksListState.Value.IsListLoading)
        {
            return;
        }

        if (_tasksListState.Value.SelectedProjectId == selectedProjectId.Value
            && _tasksListState.Value.List.Any())
        {
            return;
        }

        Dispatcher.Dispatch(new TimeTracker.Client.Core.Store.TasksList.LoadListAction(ProjectId: selectedProjectId.Value));
    }
}
