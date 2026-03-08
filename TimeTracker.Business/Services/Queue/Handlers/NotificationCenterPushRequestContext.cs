using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class NotificationCenterPushRequestContext: IQueueItemContext
{
    public NotificationActionType Action { get; set; }
    
    public Guid ProducedUserId { get; set; }
    
    public Guid? TaskCommentId { get; set; }
    
    public Guid? TaskId { get; set; }

    public NotificationCenterPushRequestContext()
    {
    }
}
