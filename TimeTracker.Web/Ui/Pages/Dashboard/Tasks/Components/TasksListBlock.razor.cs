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
            .Where(item => item.Project == _projectState.Value.Selected)
            .Where(item => string.IsNullOrWhiteSpace(_taskListSearch) || item.Name.Contains(_taskListSearch));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _tasksListState.StateChanged += SetDefaultTaskList;
        SetDefaultTaskList();
        Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.LoadListAction());
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
    }

    public void Dispose()
    {
        _projectState.StateChanged -= SetDefaultTaskList;
    }
    
    private void OnTasksListSelected(Guid? taskListId)
    {
        if (taskListId.HasValue)
        {
            var selectedProject = _tasksListState.Value.List.FirstOrDefault(item => item.Project.Id == taskListId)?.Project;
            if (selectedProject != null)
            {
                Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.SetSelectedAction(selectedProject));
            }
            Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.SetSelectedAction(taskListId));
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());    
        }
    }
    
    private void OnSelectedProject(ProjectDto? project)
    {
        if (project is null)
            return;
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.SetSelectedAction(project));
    }
    
    private string GetTaskListClass(TaskListDto? taskList)
    {
        var selected = _selectedTaskList == taskList;
        return selected
            ? "flex w-full items-center justify-between rounded-2xl border border-blue-200 bg-blue-50 px-3 py-3 text-left"
            : "flex w-full items-center justify-between rounded-2xl border border-slate-200 px-3 py-3 text-left transition hover:border-slate-300 hover:bg-slate-50";
    }
    
    private void SetDefaultTaskList(object? sender, EventArgs e)
    {
        SetDefaultTaskList();
    }

    private void SetDefaultTaskList()
    {
        if (_tasksListState.Value.SelectedTaskList == null)
        {
            var selectedTaskList = _tasksListState.Value.List.FirstOrDefault();
            if (selectedTaskList != null)
            {
                Dispatcher.Dispatch(new TimeTracker.Web.Store.Project.SetSelectedAction(selectedTaskList.Project));
                Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.SetSelectedAction(selectedTaskList.Id));
                Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
            }
        }
    }
    
    private Task OnTasksListAdded(TaskListDto arg)
    {
        OnTasksListSelected(arg.Id);
        return Task.CompletedTask;
    }
}
