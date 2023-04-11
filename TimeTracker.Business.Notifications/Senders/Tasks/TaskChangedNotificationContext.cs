using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskChangedNotificationContext : INotificationItemContext
    {
        public string ToAddress { get; set; }

        public Dictionary<string, string?> ChangeSet { get; set; }

        public TaskChangedNotificationContext() {}
    }
}
