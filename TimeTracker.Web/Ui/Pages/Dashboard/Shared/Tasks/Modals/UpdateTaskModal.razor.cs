using LumexUI;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

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
    
    private LumexModal modal;
    
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
