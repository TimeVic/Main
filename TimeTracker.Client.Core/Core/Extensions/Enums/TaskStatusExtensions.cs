using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Client.Core.Ui.Shared.Components.Enums;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Core.Core.Extensions.Enums;

public static class TaskStatusExtensions
{
    public static ComponentColor GetSelectColor(this TaskStatus status) => status switch
    {
        TaskStatus.Backlog => ComponentColor.Secondary,
        TaskStatus.Done => ComponentColor.Success,
        TaskStatus.ToDo => ComponentColor.Primary,
        _ => ComponentColor.Default
    };

    public static ComponentColor GetComponentColor(this TaskStatus status) => status.GetSelectColor();

    public static ComponentColor GetSelectColor(this ExtendedTaskStatus status) => status switch
    {
        ExtendedTaskStatus.Backlog => ComponentColor.Secondary,
        ExtendedTaskStatus.Done => ComponentColor.Success,
        ExtendedTaskStatus.InProgress => ComponentColor.Warning,
        ExtendedTaskStatus.ToDo => ComponentColor.Primary,
        _ => ComponentColor.Default
    };

    public static ComponentColor GetComponentColor(this ExtendedTaskStatus status) => status.GetSelectColor();

    public static string GetStatusCssClass(this TaskStatus status) => status switch
    {
        TaskStatus.ToDo => "todo",
        TaskStatus.Backlog => "backlog",
        TaskStatus.Done => "done",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string GetStatusCssClass(this ExtendedTaskStatus status) => status switch
    {
        ExtendedTaskStatus.ToDo => "todo",
        ExtendedTaskStatus.InProgress => "in-progress",
        ExtendedTaskStatus.Backlog => "backlog",
        ExtendedTaskStatus.Done => "done",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string GetStatusIconClass(this TaskStatus status) => status switch
    {
        TaskStatus.ToDo => "fa-solid fa-list-check",
        TaskStatus.Backlog => "fa-solid fa-layer-group",
        TaskStatus.Done => "fa-solid fa-circle-check",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string GetStatusIconClass(this ExtendedTaskStatus status) => status switch
    {
        ExtendedTaskStatus.ToDo => "fa-solid fa-list-check",
        ExtendedTaskStatus.InProgress => "fa-solid fa-person-running",
        ExtendedTaskStatus.Backlog => "fa-solid fa-layer-group",
        ExtendedTaskStatus.Done => "fa-solid fa-circle-check",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
