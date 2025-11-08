using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using TimeTracker.Business.Common.Constants.Task;

namespace TimeTracker.Web.Core.Extensions.Enums;

public static class TaskPriorityExtensions
{
    public static (string backroundColor, string textColor) GetColors(this TaskPriority status)
    {
        switch (status)
        {
            case TaskPriority.Low:
                return (Color.Neutral.GetDescription()!, "#fff");
            case TaskPriority.Medium:
                return (Color.Accent.GetDescription()!, "#fff");
            case TaskPriority.High:
                return ("#f2c811", "#000");
            case TaskPriority.Urgent:
                return ("#bc1948", "#fff");
        }

        throw new Exception($"BadgeStyle not found for TaskPriority: {status}");
    }
}
