using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.TasksList;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksListTree
{
    [Parameter]
    public long? ClientId { get; set; }
    
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    [Inject]
    public IState<TasksListState> TasksListState { get; set; }

    public ICollection<ProjectDto> Projects
    {
        get
        {
            var projects = ProjectState.Value.SortedList;
            return projects.Where(item => item.Client?.Id == ClientId).ToList();
        }
    }

    public ICollection<TaskListDto> GetTasksList(ProjectDto project)
    {
        return TasksListState.Value.List.Where(item => item.Project.Id == project.Id).ToList();
    }

    private async Task ShowAddTaskListModal()
    {
        await DialogService.OpenAsync<UpdateTasksListForm>(
            "Task list",
            options: new DialogOptions { Width = "400px", Height = "300px", Resizable = true, Draggable = false }
        );
    }
}
