using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowEditTaskModal(TaskDto task, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddTaskModal(
        TaskListDto? taskList = null,
        Guid? taskListId = null,
        Guid? projectId = null,
        Guid? timeEntryId = null,
        Action<AppModalResult>? onClose = null
    );

    Task<AppModalResult> ShowUpdateTaskListModal(TaskListDto taskList, Action<AppModalResult>? onClose = null);

    Task<AppModalResult> ShowAddTasksListModal(ProjectDto? project = null, Action<TaskListDto?>? onAdded = null, Action<AppModalResult>? onClose = null);
}
