using LumexUI.Common;
using TimeTracker.Business.Common.Constants.Task;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Core.Extensions.Enums;

public static class TaskStatusExtensions
{
    public static ThemeColor GetThemeColor(this TaskStatus status)
    {
        switch (status)
        {
            case TaskStatus.Backlog:
                return ThemeColor.Secondary;
            case TaskStatus.Done:
                return ThemeColor.Success;
            case TaskStatus.InProgress:
                return ThemeColor.Info;
            case TaskStatus.ToDo:
                return ThemeColor.Primary;
        }

        throw new Exception($"BadgeStyle not found for TaskPriority: {status}");
    }
}
