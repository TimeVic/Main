using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskChangedNotificationContext : INotificationItemContext
    {
        public Guid TaskHistoryItemId { get; set; }

        public Guid RecipientUserId { get; set; }
        
        public TaskChangedNotificationContext() {}
    }
}
