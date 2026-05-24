using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Store.Auth;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks;

public partial class TaskIdBadge
{
    [Parameter]
    public TaskDto InternalTask { get; set; }

    [Parameter]
    public bool IsClickable { get; set; } = false;

    [Parameter]
    public bool IsReplaceWithExternal { get; set; } = false;
    
    [Parameter]
    public string Class { get; set; }

    [Parameter]
    public bool IsLink { get; set; } = false;
    
    [Inject]
    public IState<TasksState> TaskState { get; set; }
    
    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    private async Task OnClick()
    {
        if (!IsClickable)
        {
            await Task.CompletedTask;
            return;
        }

        // TODO: Read task data from the server?
        var task = TaskState.Value.List.FirstOrDefault(item => item.TaskId == InternalTask.TaskId);
        task ??= InternalTask;
        // await ModalDialogService.ShowEditTaskModal(task);
    }
}
