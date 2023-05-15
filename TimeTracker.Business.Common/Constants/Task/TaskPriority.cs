using System.ComponentModel;
using TimeTracker.Business.Common.Exceptions.Api;

namespace TimeTracker.Business.Common.Constants.Task;

public enum TaskPriority
{
    [Description("Urgent")]
    Urgent = 1,
    
    [Description("High")]
    High = 2,
    
    [Description("Medium")]
    Medium = 3,
    
    [Description("Low")]
    Low = 4
}
