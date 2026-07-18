using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks.Comments
{
    public class SetCommentNotificationContext : INotificationItemContext
    {
        public string ToAddress { get; set; } = string.Empty;
        
        public Guid TaskId { get; set; }

        public Guid WorkspaceId { get; set; }
        
        public string OwnerName { get; set; } = string.Empty;
        
        public string Comment { get; set; } = string.Empty;
        
        public bool IsUpdated { get; set; }
        
        public SetCommentNotificationContext() {}
    }
}
