using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Dashboard;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using SetListItemAction = TimeTracker.Web.Store.Tasks.SetListItemAction;
using SetListItemsAction = TimeTracker.Web.Store.Tasks.SetListItemsAction;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksTable
{
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    [Inject]
    public IState<TasksListState> TasksListState { get; set; }
    
    [Inject]
    public ModalDialogProviderService ModalDialogProviderService { get; set; }
    
    [Inject]
    public IActionSubscriber ActionSubscriber { get; set; }
    
    private ICollection<TaskStatus> _statuses = new List<TaskStatus>()
    {
        TaskStatus.Backlog,
        TaskStatus.ToDo,
        TaskStatus.InProgress,
        TaskStatus.Done
    };
    
    private ICollection<TaskDto> _tasks => TasksState.Value.List;

    private async Task OnAddTask(TaskStatus status)
    {
        await ModalDialogProviderService.ShowAddTaskModal(
            taskListId: TasksListState.Value.SelectedTaskListId,
            taskStatus: status
        );
    }
}
