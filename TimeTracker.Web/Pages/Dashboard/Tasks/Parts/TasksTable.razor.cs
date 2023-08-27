using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Dashboard;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using SetListItemAction = TimeTracker.Web.Store.Tasks.SetListItemAction;
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
    
    private ICollection<TaskStatus> _statuses = new List<TaskStatus>()
    {
        TaskStatus.Backlog,
        TaskStatus.ToDo,
        TaskStatus.InProgress,
        TaskStatus.Done
    };

    private ICollection<TaskDto> _tasks => TasksState.Value.List;
    private ICollection<TaskDto> _tasksToDragAndDrop = new List<TaskDto>();

    private MudDropContainer<TaskDto> _dropContainer;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        TasksState.StateChanged += (sender, args) =>
        {
            _tasksToDragAndDrop = _tasks.OrderBy(item => item.PositionIndex).ToList();
            _dropContainer?.Refresh();
        };
    }

    private void OnClickTask(TaskDto task)
    {
        InvokeAsync(async () => await ModalDialogProviderService.ShowEditTaskModal(task));
    }

    private async Task OnAddTask(TaskStatus status)
    {
        await ModalDialogProviderService.ShowAddTaskModal(
            taskListId: TasksListState.Value.SelectedTaskListId,
            taskStatus: status
        );
    }

    private void TaskUpdated(MudItemDropInfo<TaskDto> eventData)
    {
        InvokeAsync(() =>
        {
            var currentStatus = Enum.Parse<TaskStatus>(eventData.DropzoneIdentifier);
        
            // Update positions
            var statusColumnOffset = 0;
            foreach (var status in _statuses.Where(x => x < currentStatus))
            {
                statusColumnOffset += _tasksToDragAndDrop.Count(x => x.Status == status);
            }
            _tasksToDragAndDrop.UpdateOrder(
                eventData,
                item => item.PositionIndex,
                statusColumnOffset
            );
            Dispatcher.Dispatch(new UpdatePositionsAction(_tasksToDragAndDrop));
            Dispatcher.Dispatch(new UpdateListItemsAction(_tasksToDragAndDrop));
        
            // Update status
            var updatedItem = _tasksToDragAndDrop.First(x => x.TaskId == eventData.Item.TaskId);
            updatedItem.Status = Enum.Parse<TaskStatus>(eventData.DropzoneIdentifier);
            var updateModel = new UpdateRequest();
            updateModel.Fill(updatedItem);
            Dispatcher.Dispatch(new UpdateListItemAction(updateModel, false));
            Dispatcher.Dispatch(new SetListItemAction(updatedItem));
            Dispatcher.Dispatch(new SetTasksListItemAction(updatedItem));
        });
    }

    private bool DropItemSelector(TaskDto task, string columnId)
    {
        return task.Status.ToString() == columnId;
    }
}
