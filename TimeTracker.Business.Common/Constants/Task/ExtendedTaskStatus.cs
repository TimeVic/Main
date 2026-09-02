using System.ComponentModel;

namespace TimeTracker.Business.Common.Constants.Task;

public enum ExtendedTaskStatus : short
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
