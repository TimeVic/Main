using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationItemContext : INotificationItemContext
    {
        public Guid UserId { get; set; }

        public TimeEntryAutoStoppedNotificationItemContext() {}

        public TimeEntryAutoStoppedNotificationItemContext(Guid userId)
        {
            UserId = userId;
        }
    }
}
