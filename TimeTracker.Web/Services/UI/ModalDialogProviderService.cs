using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
using TimeTracker.Web.Shared.Components.Storage;

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
            $"{task.Title.Truncate(50)}",
            parameters: new Dictionary<string, object>()
            {
                { "Task", task }
            },
            options: new DialogOptions()
            {
                Style = "top: 6em; bottom: 6em;",
                ShowClose = true,
                CloseDialogOnEsc = true,
                AutoFocusFirstElement = true,
                Resizable = false,
                CloseDialogOnOverlayClick = true,
            }
        );
    }
    
    public async Task ShowEditTaskListModal(TaskListDto? taskList = null)
    {
        if (taskList == null)
        {
            await _dialogService.OpenSideAsync<AddTasksListForm>(
                "Add task list",
                options: new SideDialogOptions { CloseDialogOnOverlayClick = true }
            );
        }
        else
        {
            await _dialogService.OpenSideAsync<UpdateTasksListForm>(
                taskList.Name,
                parameters: new Dictionary<string, object>()
                {
                    { "TaskList", taskList }
                },
                options: new SideDialogOptions { CloseDialogOnOverlayClick = true }
            );
        }
    }
    
    public async Task ShowFileView(StoredFileDto file)
    {
        await _dialogService.OpenAsync<FileView>(
            file.OriginalFileName,
            parameters: new Dictionary<string, object>()
            {
                { "File", file }
            },
            options: new DialogOptions()
            {
                Style = "top: 1em; bottom: 1em; right: 1em; left: 1em",
                Width = "auto",
                ShowClose = true,
                CloseDialogOnEsc = true,
                AutoFocusFirstElement = true,
                Resizable = true,
                CloseDialogOnOverlayClick = true,
            }
        );
    }
}
