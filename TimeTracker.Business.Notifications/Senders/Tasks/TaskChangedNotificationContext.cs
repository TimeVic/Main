using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskChangedNotificationContext : INotificationItemContext
    {
        public string UserName { get; set; } = string.Empty;
        
        public string ToAddress { get; set; } = string.Empty;

        public Dictionary<string, string?> ChangeSet { get; set; } = null!;

        public Guid TaskId { get; set; }
        
        public string TaskTitle { get; set; } = string.Empty;
        
        public TaskChangedNotificationContext() {}
    }
}
