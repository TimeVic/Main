using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksListTree
{
    [Parameter]
    public long ClientId { get; set; }

    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    [Inject]
    public IState<TasksListState> TasksListState { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; }

    private long? _nullableClientId => ClientId > 0 ? ClientId : null;
    
    public ICollection<ProjectDto> Projects
    {
        get
        {
            var projects = ProjectState.Value.List;
            return projects.Where(item => item.Client?.Id == _nullableClientId).ToList();
        }
    }
    
    public ICollection<TaskListDto> TasksList
    {
        get
        {
            var taskLists = TasksListState.Value.List;
            var projects = ProjectState.Value.List;
            return taskLists.Where(item =>
            {
                var projectWithClient = projects.FirstOrDefault(item2 => item2.Id == item.Project.Id);
                return projectWithClient?.Client?.Id == _nullableClientId;
            }).ToList();
        }
    }

    public long? _selectedTaskListId
    {
        get => TasksListState.Value.SelectedTaskListId;
    }
    
    public ICollection<TaskListDto> GetTasksList(ProjectDto project)
    {
        return TasksListState.Value.List.Where(item => item.Project.Id == project.Id).ToList();
    }

    private async Task ShowAddTaskListModal()
    {
        await DialogService.OpenSideAsync<AddTasksListForm>(
            "Add task list",
            options: new SideDialogOptions { CloseDialogOnOverlayClick = true }
        );
    }

    private void OnSelectedTestsList(object tasksListIdObject)
    {
        var tasksListId = (long) tasksListIdObject;
        
        NavigationManager.NavigateTo(
            string.Format(SiteUrl.Dashboard_Tasks, ClientId, tasksListId)    
        );
    }
}
