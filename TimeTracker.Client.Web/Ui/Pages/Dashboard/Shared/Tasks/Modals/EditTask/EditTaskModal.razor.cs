using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Web.Ui.Components.Core.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Forms;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals.EditTask;

public partial class EditTaskModal
{
    private UpdateTaskForm? _taskForm;

    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public TaskDto? Task { get; set; }

    [Parameter]
    public virtual EventCallback OnClose { get; set; }

    private System.Threading.Tasks.Task OnStatusChanged(TaskStatus status)
    {
        if (Task != null)
        {
            Task.Status = status;
        }

        return System.Threading.Tasks.Task.CompletedTask;
    }

    private System.Threading.Tasks.Task OnTitleChanged(string title)
    {
        _taskForm?.SetTitle(title);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    private async System.Threading.Tasks.Task OnCloseModal()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }

        ModalInstance?.Close(AppModalResult.Cancel("close"));
    }
}
