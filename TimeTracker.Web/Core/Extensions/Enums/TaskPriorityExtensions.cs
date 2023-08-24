using MudBlazor;
using Radzen;
using TimeTracker.Business.Common.Constants.Task;

namespace TimeTracker.Web.Core.Extensions.Enums;

public static class TaskPriorityExtensions
{
    public static BadgeStyle GetBadgeStyle(this TaskPriority status)
    {
        switch (status)
        {
            case TaskPriority.Low:
                return BadgeStyle.Light;
            case TaskPriority.Medium:
                return BadgeStyle.Info;
            case TaskPriority.High:
                return BadgeStyle.Warning;
            case TaskPriority.Urgent:
                return BadgeStyle.Danger;
        }

        throw new Exception($"BadgeStyle not found for TaskPriority: {status}");
    }
    
    public static MudBlazor.Color? GetChipStyle(this TaskPriority status)
    {
        switch (status)
        {
            case TaskPriority.Low:
                return null;
            case TaskPriority.Medium:
                return MudBlazor.Color.Info;
            case TaskPriority.High:
                return MudBlazor.Color.Warning;
            case TaskPriority.Urgent:
                return MudBlazor.Color.Error;
        }

        throw new Exception($"BadgeStyle not found for TaskPriority: {status}");
    }
}
