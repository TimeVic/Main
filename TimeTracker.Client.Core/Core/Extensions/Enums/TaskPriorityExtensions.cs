using LumexUI.Common;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Client.Core.Ui.Shared.Components.Form.Select.Core;

namespace TimeTracker.Client.Core.Core.Extensions.Enums;

public static class TaskPriorityExtensions
{
    public static SelectColor GetSelectColor(this TaskPriority priority) => priority switch
    {
        TaskPriority.Low => SelectColor.Secondary,
        TaskPriority.Medium => SelectColor.Primary,
        TaskPriority.High => SelectColor.Warning,
        TaskPriority.Urgent => SelectColor.Danger,
        _ => SelectColor.Default
    };

    public static ThemeColor GetThemeColor(this TaskPriority status)
    {
        switch (status)
        {
            case TaskPriority.Low:
                return ThemeColor.Secondary;
            case TaskPriority.Medium:
                return ThemeColor.Primary;
            case TaskPriority.High:
                return ThemeColor.Warning;
            case TaskPriority.Urgent:
                return ThemeColor.Danger;
        }

        throw new Exception($"BadgeStyle not found for TaskPriority: {status}");
    }
}
