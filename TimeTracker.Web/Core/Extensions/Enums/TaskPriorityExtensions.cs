using MudBlazor;
using TimeTracker.Business.Common.Constants.Task;

namespace TimeTracker.Web.Core.Extensions.Enums;

public static class TaskPriorityExtensions
{
    public static MudBlazor.Color GetChipStyle(this TaskPriority status)
    {
        switch (status)
        {
            case TaskPriority.Low:
                return MudBlazor.Color.Default;
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
