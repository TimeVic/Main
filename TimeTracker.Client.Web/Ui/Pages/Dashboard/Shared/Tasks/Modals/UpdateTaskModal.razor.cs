using LumexUI;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

public partial class UpdateTaskModal
{
    [Parameter]
    public required TaskDto Task { get; set; }
    
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback OnClose { get; set; }

    private LumexModal modal = null!;
    private bool _isFullScreen;
    private TaskStatus _status;

    protected override void OnParametersSet()
    {
        _status = Task.Status;
    }

    private void ToggleFullScreen()
    {
        _isFullScreen = !_isFullScreen;
    }

    private Task OnStatusChanged(TaskStatus status)
    {
        _status = status;
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
