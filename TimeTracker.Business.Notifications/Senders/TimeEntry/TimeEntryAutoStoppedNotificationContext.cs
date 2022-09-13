using System.Net;
using System.Web;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationContext : INotificationContext
    {
        public string ToAddress { get; set; }

        public TimeEntryAutoStoppedNotificationContext() {}

        public TimeEntryAutoStoppedNotificationContext(
            string toAddress    
        )
        {
            ToAddress = toAddress;
        }
    }
}
