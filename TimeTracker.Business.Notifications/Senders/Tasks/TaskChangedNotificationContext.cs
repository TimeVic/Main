using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskChangedNotificationContext : INotificationItemContext
    {
        public string UserName { get; set; }
        
        public string ToAddress { get; set; }

        public Dictionary<string, string?> ChangeSet { get; set; }

        public long TaskId { get; set; }
        
        public string TaskTitle { get; set; }
        
        public TaskChangedNotificationContext() {}
    }
}
