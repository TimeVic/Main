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

    private async Task OnClickTask(TaskDto task)
    {
        await DialogService.OpenAsync<UpdateTaskForm>(
            $"Update task #{task.Id}",
            parameters: new Dictionary<string, object>()
            {
                { "Task", task }
            },
            options: new DialogOptions()
            {
                Width = " ",
                Style = "left: 6em; right: 6em; top: 6em;",
                ShowClose = true,
                CloseDialogOnEsc = true,
                AutoFocusFirstElement = true,
                Resizable = true
            }
        );
    }
}
