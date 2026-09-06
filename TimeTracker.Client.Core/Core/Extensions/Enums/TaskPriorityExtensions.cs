using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Client.Core.Ui.Shared.Components.Enums;

namespace TimeTracker.Client.Core.Core.Extensions.Enums;

public static class TaskPriorityExtensions
{
    public static ComponentColor GetSelectColor(this TaskPriority priority) => priority switch
    {
        TaskPriority.Low => ComponentColor.Secondary,
        TaskPriority.Medium => ComponentColor.Primary,
        TaskPriority.High => ComponentColor.Warning,
        TaskPriority.Urgent => ComponentColor.Danger,
        _ => ComponentColor.Default
    };

    public static ComponentColor GetComponentColor(this TaskPriority priority) => priority.GetSelectColor();
}
