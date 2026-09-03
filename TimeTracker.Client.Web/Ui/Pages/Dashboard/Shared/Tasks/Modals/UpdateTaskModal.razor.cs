using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;

public partial class UpdateTaskModal
{
    [Parameter]
    public TaskDto? Task { get; set; }
    
    [Parameter]
    public bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback OnClose { get; set; }
}
