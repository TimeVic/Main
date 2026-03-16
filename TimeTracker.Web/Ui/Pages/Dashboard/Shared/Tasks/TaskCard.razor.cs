using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.TasksList;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks;

public partial class TaskCard
{
    [Parameter]
    public TaskDto Task { get; set; }

    [Parameter]
    public bool IsShowTaskList { get; set; } = false;
    
    [Parameter]
    public string? Class { get; set; }
    
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

    private ICollection<TaskPriority> _priorities = Enum.GetValues(typeof(TaskPriority)).Cast<TaskPriority>().ToList();

    private string _taskListPath
    {
        get
        {
            var projectName = Task.TaskList?.Project?.Name;
            var clientName = Task.TaskList?.Project?.Client?.Name;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(clientName))
                parts.Add(clientName);
            if (!string.IsNullOrEmpty(projectName))
                parts.Add(projectName);
            parts.Add(Task.TaskList!.Name);
            
            return string.Join(" > ", parts);
        }
    }

    private void OnClickTask()
    {
        InvokeAsync(async () => await ModalDialogProviderService.ShowEditTaskModal(Task));
    }
    
    private async Task OnSelectTaskPriority(TaskPriority priority)
    {
        Task.Priority = priority;
        await UpdateTask(Task);
    }
    
    private Task OnSelectTaskStatus(TaskStatus status)
    {
        Task.Status = status;
        return UpdateTask(Task);
    }
    
    private async Task UpdateTask(TaskDto task)
    {
        await InvokeAsync(() =>
        {
            var updateModel = new UpdateRequest();
            updateModel.Fill(task);
            Dispatcher.Dispatch(new UpdateTaskAction(updateModel, true));
        });
    }
}
