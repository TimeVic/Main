using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Common.Constants;

namespace TimeTracker.Business.Services.Queue.Handlers;

public class NotificationCenterPushRequestContext: IQueueItemContext
{
    public NotificationActionType Action { get; set; }
    
    public long ProducedUserId { get; set; }
    
    public long? TaskCommentId { get; set; }
    
    public long? TaskId { get; set; }

    public NotificationCenterPushRequestContext()
    {
    }
}
