using System.Net;
using System.Web;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class PasswordHasBeenChangedNotificationContext : INotificationItemContext
    {
        public string ToAddress { get; set; }
        
        public PasswordHasBeenChangedNotificationContext() {}
    }
}
