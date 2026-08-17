using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerificationNotificationItemContext : INotificationItemContext
    {
        public Guid UserId { get; set; }

        public EmailVerificationNotificationItemContext() {}

        public EmailVerificationNotificationItemContext(Guid userId)
        {
            UserId = userId;
        }
    }
}
