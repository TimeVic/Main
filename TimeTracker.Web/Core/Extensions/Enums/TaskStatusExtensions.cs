using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Web.Core.Extensions.Enums;

public static class TaskStatusExtensions
{
    public static string GetColor(this TaskStatus status)
    {
        switch (status)
        {
            case TaskStatus.Backlog:
                return "rz-color-base-800";
            case TaskStatus.Done:
                return "rz-color-success";
            case TaskStatus.InProgress:
                return "rz-color-primary";
        }

        return string.Empty;
    }
}
