using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity.Common;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Api.Shared.Dto.Entity;

public class NotificationDto: BaseDto
{   
    public virtual NotificationActionType Type { get; set; }
    
    public virtual bool IsRead { get; set; } = false;
    
    public virtual DateTime CreatedAt { get; set; }
    
    public virtual string? Comment { get; set; }
    
    public UserDto? PerformedUser { get; set; }
    
    public UserDto? ReceiverUser { get; set; }
    
    public TaskDto? Task { get; set; }
    
    public TaskCommentDto? TaskComment { get; set; }
}
