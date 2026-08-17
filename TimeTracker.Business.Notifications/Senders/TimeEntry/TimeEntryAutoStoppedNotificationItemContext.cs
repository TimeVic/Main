using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationItemContext : INotificationItemContext
    {
        public Guid TimeEntryId { get; set; }

        public TimeEntryAutoStoppedNotificationItemContext() {}

        public TimeEntryAutoStoppedNotificationItemContext(Guid timeEntryId)
        {
            TimeEntryId = timeEntryId;
        }
    }
}
