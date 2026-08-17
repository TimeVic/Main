using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskReminderNotificationContext : INotificationItemContext
    {
        public Guid TaskId { get; set; }

        public Guid RecipientUserId { get; set; }
        
        public TaskReminderNotificationContext() {}
    }
}
