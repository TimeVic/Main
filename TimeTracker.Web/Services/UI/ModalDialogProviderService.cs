using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;
using TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
// using TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;
// using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;
// using TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;
// using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
using TimeTracker.Web.Shared.Components.Dialogs;
using TimeTracker.Web.Shared.Components.Storage;
// using TimeTracker.Web.Shared.Components.Storage;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Services.UI;

public partial class ModalDialogProviderService
{
    // MudRadzen
    private readonly IDialogService _dialogService;

    public ModalDialogProviderService(
        IDialogService dialogService
    )
    {
        _dialogService = dialogService;
    }

    #region Confirmation Dialog

    public async Task<bool?> ShowDeleteConfirmationDialog(
        string message = "Are you sure you want to remove this item?"
    )
    {
        return await ShowConfirmationDialog(
            message,
            "Delete",
            Color.Error
        );
    }

    public async Task<bool?> ShowConfirmationDialog(
        string messsage,
        string buttonText = "Accept",
        Color buttonColor = Color.Accent
    )
    {
        var parameters = new DialogParameters
        {
            Width = "300px",
            PrimaryAction = "",
            TrapFocus = false
        };
        var dialog = await _dialogService.ShowDialogAsync<ConfirmationDialog>(
            new ConfirmationDialog.Parameters
            {
                Message = messsage,
                ButtonColor = buttonColor,
                ButtonText = buttonText
            },
            parameters
        );
        var result = await dialog.Result;
        return result.Data is bool;
    }
    
    #endregion

    // #region Task
    //
    public async Task ShowEditTaskModal(TaskDto task)
    {
        var parameters = new DialogParameters
        {
            Width = "900px",
            Alignment = HorizontalAlignment.Right,
            PrimaryActionEnabled = false,
            SecondaryActionEnabled = false,
            DialogType = DialogType.Panel
        };
        var dialog = await _dialogService.ShowDialogAsync<UpdateTaskModal>(
            new UpdateTaskModal.Parameters
            {
                Task = task
            },
            parameters
        );
    }
    
    public async Task ShowEditTaskListModal(
        TaskListDto? taskList = null,
        long? projectId = null
    )
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            DialogType = DialogType.Dialog,
            TrapFocus = false
        };
        
        if (taskList == null)
        {
            await _dialogService.ShowDialogAsync<AddTasksListModalForm>(
                new AddTasksListModalForm.Parameters
                {
                    ProjectId = projectId
                },
                parameters
            );
        }
        else
        {
            await _dialogService.ShowDialogAsync<UpdateTasksListModalForm>(
                new UpdateTasksListModalForm.Parameters
                {
                    TaskList = taskList
                },
                parameters
            );
        }
    }
    
    public async Task ShowAddTaskModal(
        long? timEntryId = null,
        long? taskListId = null,
        DateTime? endTime = null,
        TaskStatus? taskStatus = null
    )
    {
        var parameters = new DialogParameters
        {
            Width = "500px",
            PrimaryAction = ""
        };
        await _dialogService.ShowDialogAsync<AddTaskModalForm>(
            new AddTaskModalForm.Parameters
            {
                TimeEntryId = timEntryId,
                TaskListId = taskListId,
                TaskStatus = taskStatus,
                EndTime = endTime,
            },
            parameters
        );
    }
    //
    // #endregion

    public async Task ShowFileView(StoredFileDto file)
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            DialogType = DialogType.Dialog,
            TrapFocus = false
        };
        await _dialogService.ShowDialogAsync<FileViewModal>(
            new FileViewModal.Parameters
            {
                File = file
            },
            parameters
        );
    }
    
    public async Task ShowTimeEntriesModal()
    {
        var parameters = new DialogParameters
        {
            PrimaryAction = "",
            DialogType = DialogType.SplashScreen,
            Width = "100%",
            TrapFocus = false
        };
        await _dialogService.ShowDialogAsync<TimeEntryListModal>(
            new TimeEntryListModal.Parameters
            {
                IsFilteredList = false
            },
            parameters
        );
        
    }
}
