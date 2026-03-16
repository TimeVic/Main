using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Common;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Tasks.Parts;

internal enum TaskListAction
{
    Add = -1,
    Edit = -2,
    Delete = -3,
}

public partial class TasksListTree
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
    
    [Inject]
    public ModalDialogProviderService _modalDialogProviderService { get; set; }

    private Guid _projectId = Guid.Empty;
    private Guid? _taskListId = null;
    private bool _isTaskListsMenuOpened = false;
    private bool _isTaskListMenuOpened = false;
    
    public TaskListDto? _selectedTaskList
    {
        get => _tasksListState.Value.SelectedTaskList;
    }
    
    private ICollection<TaskListDto> _projectTasksList
    {
        get
        {
            var list = _tasksListState.Value.List;
            return list.Where(item => item.Project?.Id == _projectId).ToList();
        }
    }
    
    private IEnumerable<IGrouping<Guid, TaskListDto>> _groupedTasksList
    {
        get
        {
            return _tasksListState.Value.List.GroupBy(item => item.Project.Id);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private void ShowAddTaskListModal()
    {
        InvokeAsync(async () => await _modalDialogProviderService.ShowEditTaskListModal(projectId: null));
    }
    
    private void ShowUpdateTaskListModal()
    {
        InvokeAsync(async () => await _modalDialogProviderService.ShowEditTaskListModal(_selectedTaskList));
    }

    private async Task ShowAddTaskModal()
    {
        await _modalDialogProviderService.ShowAddTaskModal(taskListId: _taskListId);
    }
    
    private async Task OnDeleteTaskList()
    {
        var isOk = await _modalDialogProviderService.ShowDeleteConfirmationDialog(
            $"Are you sure you want to remove: {_selectedTaskList!.Name}?"
        );
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new ArchiveTaskListAction(_selectedTaskList));
        }
    }
    
    private void OnSelectedProject(ProjectDto? project)
    {
        if (project is null)
            return;
        _projectId = project.Id;
        _taskListId = null;
    }
    
    private void OnSelectedTasksList(TaskListDto? tasksList)
    {
        if (tasksList is null)
            return;
        NavigationManager.NavigateTo(
            string.Format(SiteUrl.Dashboard_Tasks, tasksList.Id)    
        );
    }
    
    private void OnTasksListSelected(Guid? tasksListId)
    {
        if (tasksListId.HasValue)
        {
            Dispatcher.Dispatch(new SetSelectedAction(tasksListId));
            Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());    
        }
    }
}
