using LumexUI.Common;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Client.Core.Core.Extensions.Enums;

public static class TaskStatusExtensions
{
    public static SelectColor GetSelectColor(this TaskStatus status) => status switch
    {
        TaskStatus.Backlog => SelectColor.Secondary,
        TaskStatus.Done => SelectColor.Success,
        TaskStatus.ToDo => SelectColor.Primary,
        _ => SelectColor.Default
    };

    public static SelectColor GetSelectColor(this ExtendedTaskStatus status) => status switch
    {
        ExtendedTaskStatus.Backlog => SelectColor.Secondary,
        ExtendedTaskStatus.Done => SelectColor.Success,
        ExtendedTaskStatus.InProgress => SelectColor.Warning,
        ExtendedTaskStatus.ToDo => SelectColor.Primary,
        _ => SelectColor.Default
    };

    public static ThemeColor GetThemeColor(this TaskStatus status)
    {
        switch (status)
        {
            case TaskStatus.Backlog:
                return ThemeColor.Secondary;
            case TaskStatus.Done:
                return ThemeColor.Success;
            case TaskStatus.ToDo:
                return ThemeColor.Primary;
        }

        throw new Exception($"BadgeStyle not found for TaskStatus: {status}");
    }

    public static ThemeColor GetThemeColor(this ExtendedTaskStatus status)
    {
        switch (status)
        {
            case ExtendedTaskStatus.Backlog:
                return ThemeColor.Secondary;
            case ExtendedTaskStatus.Done:
                return ThemeColor.Success;
            case ExtendedTaskStatus.InProgress:
                return ThemeColor.Warning;
            case ExtendedTaskStatus.ToDo:
                return ThemeColor.Primary;
        }

        throw new Exception($"BadgeStyle not found for ExtendedTaskStatus: {status}");
    }

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
