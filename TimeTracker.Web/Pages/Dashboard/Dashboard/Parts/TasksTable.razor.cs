using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Dashboard;
using TimeTracker.Web.Store.Dashboard.Effects;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Dashboard.Parts;

public partial class TasksTable
{
    [Inject]
    public IState<DashboardState> DashboardState { get; set; }

    public MyTasksState MyTasksState => DashboardState.Value.MyTasks;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Dispatcher.Dispatch(new LoadTasksListAction());
    }

    private void OnClickTask(TaskDto task)
    {
        InvokeAsync(async () => await ModalDialogProviderService.ShowEditTaskModal(task));
    }
}
