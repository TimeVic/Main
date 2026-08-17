using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks.Comments
{
    public class SetCommentNotificationContext : INotificationItemContext
    {
        public Guid TaskCommentId { get; set; }

        public Guid RecipientUserId { get; set; }
        
        public SetCommentNotificationContext() {}
    }
}
