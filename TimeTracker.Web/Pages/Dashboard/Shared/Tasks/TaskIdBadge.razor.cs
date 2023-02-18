using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class TaskIdBadge
{
    [Parameter]
    public TaskDto InternalTask { get; set; }

    [Parameter]
    public bool IsClickable { get; set; } = false;

    [Parameter]
    public string Class { get; set; }
    
    [Inject]
    public IState<TasksState> TaskState { get; set; }
    
    private async Task OnClick()
    {
        if (!IsClickable)
        {
            await Task.CompletedTask;
            return;
        }

        // TODO: Read task data from the server?
        var task = TaskState.Value.List.FirstOrDefault(item => item.Id == InternalTask.Id);
        task ??= InternalTask;
        await ModalDialogProviderService.ShowEditTaskModal(task);
    }
}
