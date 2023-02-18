using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

namespace TimeTracker.Web.Services.UI;

public class ModalDialogProviderService
{
    private readonly DialogService _dialogService;

    public ModalDialogProviderService(DialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public async Task ShowEditTaskModal(TaskDto task)
    {
        await _dialogService.OpenAsync<UpdateTaskForm>(
            $"Task {task.Id}",
            parameters: new Dictionary<string, object>()
            {
                { "Task", task }
            },
            options: new DialogOptions()
            {
                Width = " ",
                Style = "left: 6em; right: 6em; top: 6em; bottom: 6em;",
                ShowClose = true,
                CloseDialogOnEsc = true,
                AutoFocusFirstElement = true,
                Resizable = true,
                CloseDialogOnOverlayClick = true
            }
        );
    }
}
