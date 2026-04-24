using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Common;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Components;

public partial class TasksListBlock: IDisposable
{
    [CascadingParameter(Name = "TaskListId")]
    public Guid? TaskListId
    {
        get => _taskListId;
        set
        {
            _taskListId = value;
            OnTasksListSelected(_taskListId);
        }
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
    private bool _isTaskListsMenuOpened = false;
    private bool _isTaskListMenuOpened = false;
    private bool _isAddTaskListModalOpened = false;
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
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
        }
    }

    public void Dispose()
    {
        _projectState.StateChanged -= OnProjectStateChanged;
        _tasksListState.StateChanged -= OnTasksListStateChanged;
    }
    
    private void OnTasksListSelected(Guid? taskListId)
    {
        if (!taskListId.HasValue)
        {
            return;
        }

        var taskList = _tasksListState.Value.List.FirstOrDefault(item => item.Id == taskListId.Value);
        if (taskList?.Project != null)
        {
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.SetSelectedAction(taskList.Project));
        }

        Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.SetSelectedAction(taskListId));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
    }
    
    private void OnSelectedProject(ProjectDto? project)
    {
        if (project is null)
        {
            return;
        }

        Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.SetSelectedAction(project));

        if (_tasksListState.Value.SelectedTaskList?.Project?.Id != project.Id)
        {
            Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.SetSelectedAction(null));
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
        }
    }
    
    private string GetTaskListClass(TaskListDto? taskList)
    {
        var selected = _selectedTaskList == taskList;
        return selected
            ? "flex w-full items-center justify-between rounded-2xl border border-blue-200 bg-blue-50 px-3 py-3 text-left"
            : "flex w-full items-center justify-between rounded-2xl border border-slate-200 px-3 py-3 text-left transition hover:border-slate-300 hover:bg-slate-50";
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
        if (_projectState.Value.Selected == null && _tasksListState.Value.SelectedTaskList?.Project != null)
        {
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.SetSelectedAction(_tasksListState.Value.SelectedTaskList.Project));
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
            Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.SetSelectedAction(firstProjectTaskList.Id));
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
            return;
        }

        if (_tasksListState.Value.SelectedTaskListId != null)
        {
            Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.SetSelectedAction(null));
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
        }
    }
    
    private Task OnTasksListAdded(TaskListDto arg)
    {
        OnTasksListSelected(arg.Id);
        return Task.CompletedTask;
    }

    private void LoadSelectedProjectTaskLists()
    {
        var selectedProjectId = _projectState.Value.Selected?.Id;
        if (!selectedProjectId.HasValue)
        {
            if (_tasksListState.Value.List.Any() || _tasksListState.Value.SelectedProjectId.HasValue)
            {
                Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.LoadListAction(ProjectId: null));
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

        Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.LoadListAction(ProjectId: selectedProjectId.Value));
    }
}
