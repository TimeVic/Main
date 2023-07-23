using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;
using TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;
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

    #region Confirmation Dialog
    
    public async Task<bool?> ShowDeleteConfirmationDialog(string messsage = "Are you sure you want to remove this item?")
    {
        return await _dialogService.Confirm(
            messsage,
            "Delete confirmation",
            new ConfirmOptions()
            {
                OkButtonText = "Delete",
                CancelButtonText = "Cancel"
            }
        );
    }
    
    #endregion
    
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
                Style = "top: 6em; bottom: 6em; min-width: 1100px;",
                ShowClose = true,
                CloseDialogOnEsc = true,
                AutoFocusFirstElement = true,
                Resizable = false,
                CloseDialogOnOverlayClick = true
            }
        );
    }
    
    public async Task ShowEditTaskListModal(
        TaskListDto? taskList = null,
        long? projectId = null
    )
    {
        if (taskList == null)
        {
            await _dialogService.OpenSideAsync<AddTasksListForm>(
                "Add task list",
                parameters: new Dictionary<string, object>()
                {
                    { "ProjectId", projectId }
                },
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
    
    public async Task ShowAddTaskModal(
        long? timEntryId = null,
        long? taskListId = null
    )
    {
        await _dialogService.OpenAsync<AddTaskModalForm>(
            "Add new task",
            options: new DialogOptions { Width = "500px", Height = "250px", Resizable = true, Draggable = false },
            parameters: new Dictionary<string, object?>()
            {
                { "TimeEntryId", timEntryId },
                { "TaskListId", taskListId },
            }
        );
    }
    
    public async Task ShowTimeEntriesModal()
    {
        await _dialogService.OpenAsync<TimeEntryList>(
            "My Time Entries",
            options: new DialogOptions
            {
                Style = "top: 1em; bottom: 1em; right: 1em; left: 1em",
                Width = "auto",
                ShowClose = true,
                CloseDialogOnEsc = true,
                Resizable = true,
                Draggable = false
            },
            parameters: new Dictionary<string, object?>()
            {
                { "IsFilteredList", false },
            }
        );
    }
}
