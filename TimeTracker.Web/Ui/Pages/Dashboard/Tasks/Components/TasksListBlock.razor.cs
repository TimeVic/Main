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
    
    private string? _taskListSearch = null;
    private ProjectDto? _selectedProject = null;
    private Guid? _taskListId = null;
    private bool _isTaskListsMenuOpened = false;
    private bool _isTaskListMenuOpened = false;
    private bool _isAddTaskListModalOpened = false;
    
    public TaskListDto? _selectedTaskList
    {
        get => _tasksListState.Value.SelectedTaskList;
    }
    
    public IEnumerable<TaskListDto> _filteredTaskLists
    {
        get => _tasksListState.Value.List
            .Where(item => item.Project == _selectedProject)
            .Where(item => string.IsNullOrWhiteSpace(_taskListSearch) || item.Name.Contains(_taskListSearch));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _projectState.StateChanged += SetDefaultProject;
        Dispatcher.Dispatch(new TimeTracker.Web.Store.TasksList.LoadListAction());
    }

    public void Dispose()
    {
        _projectState.StateChanged -= SetDefaultProject;
    }
    
    private void OnTasksListSelected(Guid? taskListId)
    {
        if (taskListId.HasValue)
        {
            var selectedProject = _tasksListState.Value.List.FirstOrDefault(item => item.Project.Id == taskListId)?.Project;
            if (selectedProject != null)
            {
                _selectedProject = selectedProject;
            }
            Dispatcher.Dispatch(new SetSelectedAction(taskListId));
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());    
        }
    }
    
    private void OnSelectedProject(ProjectDto? project)
    {
        if (project is null)
            return;
        _selectedProject = project;
        _taskListId = null;
    }
    
    private string GetTaskListClass(TaskListDto? taskList)
    {
        var selected = _selectedTaskList == taskList;
        return selected
            ? "flex w-full items-center justify-between rounded-2xl border border-blue-200 bg-blue-50 px-3 py-3 text-left"
            : "flex w-full items-center justify-between rounded-2xl border border-slate-200 px-3 py-3 text-left transition hover:border-slate-300 hover:bg-slate-50";
    }
    
    private void SetDefaultProject(object? sender, EventArgs e)
    {
        if (_selectedProject == null)
        {
            _selectedProject = _projectState.Value.List.FirstOrDefault();
        }
    }

    private Task OnTasksListAdded(TaskListDto arg)
    {
        OnTasksListSelected(arg.Id);
        return Task.CompletedTask;
    }
}
