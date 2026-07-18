using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Forms;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

public partial class UpdateTaskModal
{
    private UpdateTaskForm? _taskForm;

    [Parameter]
    public required TaskDto Task { get; set; }
    
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback OnClose { get; set; }

    private Task OnStatusChanged(TaskStatus status)
    {
        Task.Status = status;
        return System.Threading.Tasks.Task.CompletedTask;
    }

    private Task OnTitleChanged(string title)
    {
        _taskForm?.SetTitle(title);
        return System.Threading.Tasks.Task.CompletedTask;
    }
    
    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }

    private void OnOpenChanged(bool isOpened)
    {
        if (!isOpened)
        {
            OnClose.InvokeAsync();
        }
    }
}
