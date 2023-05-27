using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.Tasks.Comments
{
    public class SetCommentNotificationContext : INotificationItemContext
    {
        public string ToAddress { get; set; }
        
        public long TaskId { get; set; }
        
        public string OwnerName { get; set; }
        
        public string Comment { get; set; }
        
        public bool IsUpdated { get; set; }
        
        public SetCommentNotificationContext() {}
    }
}
