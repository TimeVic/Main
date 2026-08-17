using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class PasswordHasBeenChangedNotificationContext : INotificationItemContext
    {
        public Guid UserId { get; set; }
        
        public PasswordHasBeenChangedNotificationContext() {}
    }
}
