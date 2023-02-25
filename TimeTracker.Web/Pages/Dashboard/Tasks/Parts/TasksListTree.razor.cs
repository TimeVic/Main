using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksListTree
{
    [Parameter]
    public long ClientId { get; set; }

    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Inject]
    public IState<TasksListState> _tasksListState { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }
    
    [Inject]
    public ModalDialogProviderService _modalDialogProviderService { get; set; }

    private long? _nullableClientId => ClientId > 0 ? ClientId : null;
    
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

    public long? _selectedTaskListId
    {
        get => _tasksListState.Value.SelectedTaskListId;
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
    
    private void OnSelectedTestsList(object tasksListIdObject)
    {
        var tasksListId = (long) tasksListIdObject;
        
        NavigationManager.NavigateTo(
            string.Format(SiteUrl.Dashboard_Tasks, ClientId, tasksListId)    
        );
    }
}
