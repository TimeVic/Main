using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Modals.EditTask;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Tasks.Parts.TasksList;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowEditTaskModal(TaskDto task, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<EditTaskModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(EditTaskModal.Task)] = task
            },
            options: new AppModalOptions
            {
                Size = AppModalSize.ExtraLarge,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowAddTaskModal(
        TaskListDto? taskList = null,
        Guid? taskListId = null,
        Guid? projectId = null,
        Guid? timeEntryId = null,
        Action<AppModalResult>? onClose = null
    )
    {
        var parameters = new Dictionary<string, object?>();
        if (taskList != null) parameters[nameof(AddTaskModal.TaskList)] = taskList;
        if (taskListId.HasValue) parameters[nameof(AddTaskModal.TaskListId)] = taskListId.Value;
        if (projectId.HasValue) parameters[nameof(AddTaskModal.ProjectId)] = projectId.Value;
        if (timeEntryId.HasValue) parameters[nameof(AddTaskModal.TimeEntryId)] = timeEntryId.Value;

        return _appModalDialogService.ShowAsync<AddTaskModal>(
            parameters: parameters,
            options: new AppModalOptions
            {
                Size = AppModalSize.Large,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowUpdateTaskListModal(TaskListDto taskList, Action<AppModalResult>? onClose = null)
    {
        return _appModalDialogService.ShowAsync<UpdateTaskListModal>(
            parameters: new Dictionary<string, object?>
            {
                [nameof(UpdateTaskListModal.TaskList)] = taskList
            },
            options: new AppModalOptions
            {
                Size = AppModalSize.Large,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowAddTasksListModal(ProjectDto? project = null, Action<TaskListDto?>? onAdded = null, Action<AppModalResult>? onClose = null)
    {
        var parameters = new Dictionary<string, object?>();
        if (project != null) parameters[nameof(AddTasksListModalForm.Project)] = project;
        if (onAdded != null)
        {
            parameters[nameof(AddTasksListModalForm.OnAdded)] = Microsoft.AspNetCore.Components.EventCallback.Factory.Create<TaskListDto?>(this, onAdded);
        }

        return _appModalDialogService.ShowAsync<AddTasksListModalForm>(
            parameters: parameters,
            options: new AppModalOptions
            {
                Size = AppModalSize.Medium,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }
}
