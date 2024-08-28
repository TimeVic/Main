using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Business.Common.Constants.Task;

namespace TimeTracker.Web.Core.Extensions.Enums;

public static class TaskPriorityExtensions
{
    public static Color GetChipStyle(this TaskPriority status)
    {
        switch (status)
        {
            case TaskPriority.Low:
                return Color.Neutral;
            case TaskPriority.Medium:
                return Color.Info;
            case TaskPriority.High:
                return Color.Warning;
            case TaskPriority.Urgent:
                return Color.Error;
        }

        throw new Exception($"BadgeStyle not found for TaskPriority: {status}");
    }
}
