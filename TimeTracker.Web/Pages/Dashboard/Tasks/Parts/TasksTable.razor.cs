using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
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
        TaskStatus.InProgress,
        TaskStatus.ToDo,
        TaskStatus.Backlog,
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

    private void OnClickTask(TaskDto task)
    {
        InvokeAsync(async () => await ModalDialogProviderService.ShowEditTaskModal(task));
    }
}
