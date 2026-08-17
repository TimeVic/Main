using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class RegistrationNotificationItemContext : INotificationItemContext
    {
        public Guid UserId { get; set; }
        
        public RegistrationNotificationItemContext() {}

        public RegistrationNotificationItemContext(Guid userId)
        {
            UserId = userId;
        }
    }
}
