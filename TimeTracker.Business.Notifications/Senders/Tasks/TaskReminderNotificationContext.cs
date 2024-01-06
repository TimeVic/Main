using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskReminderNotificationContext : INotificationItemContext
    {
        public string UserName { get; set; }
        
        public string ToEmailAddress { get; set; }

        public ICollection<string> GcmTokens { get; set; } = new List<string>();

        public long TaskId { get; set; }
        
        public long WorkspaceId { get; set; }
        
        public string TaskTitle { get; set; }
        
        public TaskReminderNotificationContext() {}
    }
}
