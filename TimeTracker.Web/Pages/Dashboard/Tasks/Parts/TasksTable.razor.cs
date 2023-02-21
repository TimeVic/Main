using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TasksTable
{
    [Inject]
    public IState<TasksState> TasksState { get; set; }

    private void OnLoadData(LoadDataArgs arg)
    {
        Dispatcher.Dispatch(new LoadListAction(arg.Skip ?? 0));
    }

    private void OnClickTask(TaskDto task)
    {
        InvokeAsync(async () => await ModalDialogProviderService.ShowEditTaskModal(task));
    }
}
