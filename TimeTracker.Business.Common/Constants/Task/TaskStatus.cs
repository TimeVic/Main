using System.ComponentModel;
using TimeTracker.Business.Common.Exceptions.Api;

namespace TimeTracker.Business.Common.Constants.Task;

public enum TaskStatus
{
    [Description("Backlog")]
    Backlog = 1,
    
    [Description("To Do")]
    ToDo = 2,
    
    [Description("In Progress")]
    InProgress = 3,
    
    [Description("Done")]
    Done = 4
}
