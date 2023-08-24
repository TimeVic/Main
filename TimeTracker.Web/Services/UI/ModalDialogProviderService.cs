using MudBlazor;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Pages.Dashboard.Shared.Tasks;
using TimeTracker.Web.Pages.Dashboard.Shared.TimeEntry;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts;
using TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;
using TimeTracker.Web.Shared.Components.Dialogs;
using TimeTracker.Web.Shared.Components.Storage;
using DialogOptions = Radzen.DialogOptions;
using DialogService = Radzen.DialogService;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Services.UI;

public class ModalDialogProviderService
{
    // Blazor
    private readonly DialogService _dialogService;

    // MudRadzen
    private readonly IDialogService _mudDialogService;

    public ModalDialogProviderService(
        DialogService dialogService,
        IDialogService mudDialogService
    )
    {
        _mudDialogService = mudDialogService;
        _dialogService = dialogService;
    }

    #region Confirmation Dialog

    public async Task<bool?> ShowDeleteConfirmationDialog(
        string messsage = "Are you sure you want to remove this item?")
    {
        var parameters = new DialogParameters<ConfirmationDialog>
        {
            {x => x.Message, messsage},
            {x => x.ButtonColor, MudBlazor.Color.Error},
            {x => x.ButtonText, "Delete"}
        };
        var dialog = await _mudDialogService.ShowAsync<ConfirmationDialog>(
            "Delete confirmation",
            parameters,
            new MudBlazor.DialogOptions()
            {
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraSmall
            }
        );
        var result = await dialog.Result;
        return !result.Canceled;
    }

    #endregion

    public async Task ShowEditTaskModal(TaskDto task)
    {
        var parameters = new DialogParameters<UpdateTaskForm>
        {
            {context => context.Task, task}
        };
        await _mudDialogService.ShowAsync<UpdateTaskForm>(
            $"{task.Title.Truncate(100)}", 
            parameters,
            new MudBlazor.DialogOptions()
            {
                CloseOnEscapeKey = true
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
            var parameters = new MudBlazor.DialogParameters<AddTasksListModalForm>
            {
                {context => context.ProjectId, projectId}
            };
            await _mudDialogService.ShowAsync<AddTasksListModalForm>("Add task list", parameters);
        }
        else
        {
            var parameters = new MudBlazor.DialogParameters<UpdateTasksListModalForm>
            {
                {context => context.TaskList, taskList}
            };
            await _mudDialogService.ShowAsync<UpdateTasksListModalForm>(taskList.Name, parameters);
        }
    }

    public async Task ShowFileView(StoredFileDto file)
    {
        var parameters = new DialogParameters<FileView>
        {
            {context => context.File, file}
        };
        await _mudDialogService.ShowAsync<FileView>(
            file.OriginalFileName, 
            parameters,
            new MudBlazor.DialogOptions()
            {
                CloseOnEscapeKey = true,
                FullWidth = true
            }
        );
    }

    public async Task ShowAddTaskModal(
        long? timEntryId = null,
        long? taskListId = null,
        TaskStatus? taskStatus = null
    )
    {
        var parameters = new DialogParameters<AddTaskModalForm>
        {
            { x => x.TimeEntryId, timEntryId },
            { x => x.TaskListId, taskListId },
            { x => x.TaskStatus, taskStatus },
        };
        await _mudDialogService.ShowAsync<AddTaskModalForm>("Add new task", parameters);
    }

    public async Task ShowTimeEntriesModal()
    {
        var parameters = new DialogParameters<TimeEntryList>
        {
            {context => context.IsFilteredList, false}
        };
        await _mudDialogService.ShowAsync<TimeEntryList>(
            "My Time Entries", 
            parameters,
            new MudBlazor.DialogOptions()
            {
                CloseOnEscapeKey = true,
                FullWidth = true
            }
        );
    }
}
