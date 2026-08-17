using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationItemContext : INotificationItemContext
    {
        public Guid UserId { get; set; }
        
        public EmailVerifiedNotificationItemContext() {}

        public EmailVerifiedNotificationItemContext(Guid userId)
        {
            UserId = userId;
        }
    }
}
