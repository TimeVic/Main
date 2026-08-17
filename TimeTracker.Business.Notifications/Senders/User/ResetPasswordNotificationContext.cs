using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class ResetPasswordNotificationContext : INotificationItemContext
    {
        public Guid ResetPasswordRequestId { get; set; }

        public ResetPasswordNotificationContext() {}
    }
}
