using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskReminderNotificationContext : INotificationItemContext
    {
        public string UserName { get; set; } = string.Empty;
        
        public string ToEmailAddress { get; set; } = string.Empty;

        public ICollection<string> GcmTokens { get; set; } = new List<string>();

        public Guid TaskId { get; set; }
        
        public Guid WorkspaceId { get; set; }
        
        public string TaskTitle { get; set; } = string.Empty;
        
        public TaskReminderNotificationContext() {}
    }
}
