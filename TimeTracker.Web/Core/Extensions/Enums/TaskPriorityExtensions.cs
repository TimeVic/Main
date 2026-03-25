using LumexUI.Common;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using TimeTracker.Business.Common.Constants.Task;

namespace TimeTracker.Web.Core.Extensions.Enums;

public static class TaskPriorityExtensions
{
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
