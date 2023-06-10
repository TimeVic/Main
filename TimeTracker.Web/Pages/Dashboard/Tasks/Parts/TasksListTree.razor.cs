using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksListTree
{
    [CascadingParameter(Name = "ClientId")]
    public long ClientId
    {
        get => _clientId;
        set
        {
            _clientId = value;
            OnTasksListSelected(null);
        }
    }
    
    [CascadingParameter(Name = "TaskListId")]
    public long? TaskListId
    {
        get => _taskListId;
        set
        {
            _taskListId = value;
            OnTasksListSelected(_taskListId);
        }
    }
    
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }
    
    [Inject]
    public ModalDialogProviderService _modalDialogProviderService { get; set; }

    private long? _nullableClientId => ClientId > 0 ? ClientId : null;
    private long _clientId = 0;
    private long? _taskListId = null;
    
    public ICollection<ProjectDto> Projects
    {
        get
        {
            var projects = _projectState.Value.List;
            return projects.Where(item => item.Client?.Id == _nullableClientId).ToList();
        }
    }
    
    public ICollection<TaskListDto> TasksList
    {
        get
        {
            var taskLists = _tasksListState.Value.List;
            var projects = _projectState.Value.List;
            return taskLists.Where(item =>
            {
                var projectWithClient = projects.FirstOrDefault(item2 => item2.Id == item.Project.Id);
                return projectWithClient?.Client?.Id == _nullableClientId;
            }).ToList();
        }
    }

    public long _selectedTaskListId
    {
        get => _tasksListState.Value.SelectedTaskListId ?? 0;
    }
    
    public ICollection<TaskListDto> GetTasksList(ProjectDto project)
    {
        return _tasksListState.Value.List.Where(item => item.Project.Id == project.Id).ToList();
    }

    private void ShowAddTaskListModal()
    {
        InvokeAsync(async () => await _modalDialogProviderService.ShowEditTaskListModal());
    }
    
    private void ShowUpdateTaskListModal()
    {
        var taskList = _tasksListState.Value.List.First(item => item.Id == _selectedTaskListId);
        InvokeAsync(async () => await _modalDialogProviderService.ShowEditTaskListModal(taskList));
    }

    private async Task OnDeleteTaskList()
    {
        var taskList = _tasksListState.Value.List.First(item => item.Id == _selectedTaskListId);
        var isOk = await DialogService.Confirm(
            "Are you sure you want to remove this task list?",
            "Delete confirmation",
            new ConfirmOptions()
            {
                OkButtonText = "Delete",
                CancelButtonText = "Cancel"
            }
        );
        if (isOk.HasValue && isOk.Value)
        {
            Dispatcher.Dispatch(new ArchiveTaskListAction(taskList));
        }
    }
    
    private void OnSelectedTasksList(long tasksListId)
    {
        NavigationManager.NavigateTo(
            string.Format(SiteUrl.Dashboard_Tasks, ClientId, tasksListId)    
        );
    }
    
    private void OnTasksListSelected(long? testsListId)
    {
        Dispatcher.Dispatch(new SetSelectedAction(testsListId));
        Dispatcher.Dispatch(new TimeTracker.Web.Store.Tasks.LoadListAction());
    }
}
