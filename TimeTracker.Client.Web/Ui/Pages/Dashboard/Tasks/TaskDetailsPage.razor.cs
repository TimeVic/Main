using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Forms;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks;

public partial class TaskDetailsPage
{
    private UpdateTaskForm? _taskForm;

    [Parameter]
    public Guid TaskId { get; set; }

    private bool _isLoading;
    private TaskFullDto? _task;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _task = await ApiService.TasksGetOneAsync(TaskId);
        _isLoading = false;
    }

    private Task OnStatusChanged(TaskStatus status)
    {
        if (_task != null)
        {
            _task.Status = status;
        }

        return Task.CompletedTask;
    }

    private Task OnTitleChanged(string title)
    {
        _taskForm?.SetTitle(title);
        return Task.CompletedTask;
    }
}
