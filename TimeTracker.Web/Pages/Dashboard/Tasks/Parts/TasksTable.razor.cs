using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksTable
{
    [Inject]
    public IState<TasksState> TasksState { get; set; }
    
    private ICollection<TaskStatus> _statusOrder = new List<TaskStatus>()
    {
        TaskStatus.Backlog,
        TaskStatus.ToDo,
        TaskStatus.InProgress,
        TaskStatus.Done
    };

    private IDictionary<TaskStatus, ICollection<TaskDto>> _taskGroups
    {
        get
        {
            var tasks = TasksState.Value.List;
            var result = new Dictionary<TaskStatus, ICollection<TaskDto>>();
            foreach (var taskStatus in _statusOrder)
            {
                result.Add(
                    taskStatus, 
                    tasks.Where(item => item.Status == taskStatus).ToList()
                );
            }

            return result;
        }
    }

    private TaskDto? _draggableTask;
    
    private void OnClickTask(TaskDto task)
    {
        InvokeAsync(async () => await ModalDialogProviderService.ShowEditTaskModal(task));
    }

    private void OnDragStart(TaskDto item)
    {
        _draggableTask = item;
    }

    private void HandleDrop(TaskStatus newStatus)
    {
        if (_draggableTask == null || _draggableTask?.Status == newStatus)
        {
            _draggableTask = null;
            return;
        }
        _draggableTask.Status = newStatus;
        Dispatcher.Dispatch(new SetListItemAction(_draggableTask));

        var updateModel = new UpdateRequest();
        updateModel.Fill(_draggableTask);
        Dispatcher.Dispatch(new UpdateListItemAction(updateModel));
        
        _draggableTask = null;
    }
}
