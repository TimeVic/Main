using Fluxor;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;
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

    private ICollection<TaskDto> _tasks => TasksState.Value.List.OrderBy(item => item.PositionIndex).ToList();
    private ICollection<TaskDto> _dropZoneTasks = new List<TaskDto>();

    private TaskDto? _draggableTask;
    private MudDropContainer<TaskDto> _dropContainer;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        TasksState.StateChanged += (sender, args) =>
        {
            Debug.Log("TasksState.StateChanged");
            _dropZoneTasks = _tasks.OrderBy(x => x.PositionIndex).ToList();
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
        var currentStatus = Enum.Parse<TaskStatus>(eventData.DropzoneIdentifier);
        
        eventData.Item.Status = Enum.Parse<TaskStatus>(eventData.DropzoneIdentifier);
        // var updateModel = new UpdateRequest();
        // updateModel.Fill(eventData.Item);
        // Dispatcher.Dispatch(new UpdateListItemAction(updateModel));

        var statusColumnOffset = 0;
        foreach (var status in _statuses.Where(x => x < currentStatus))
        {
            statusColumnOffset += _tasks.Count(x => x.Status == status);
        }
        _dropZoneTasks.UpdateOrder(
            eventData,
            item => item.PositionIndex,
            statusColumnOffset
        );
        // eventData.Item.OrderPosition = eventData.IndexInZone + statusColumnOffset;
        // Dispatcher.Dispatch(new SetListItemAction(eventData.Item));
    }

    private bool DropItemSelector(TaskDto task, string columnId)
    {
        return task.Status.ToString() == columnId;
    }
}
