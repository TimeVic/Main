using System.Net;
using System.Web;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationItemContext : INotificationItemContext
    {
        public string ToAddress { get; set; }

        public TimeEntryAutoStoppedNotificationItemContext() {}

        public TimeEntryAutoStoppedNotificationItemContext(
            string toAddress    
        )
        {
            ToAddress = toAddress;
        }
    }
}
