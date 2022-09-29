using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders
{
    public class TestNotificationItemContext : INotificationItemContext
    {
        public TestNotificationItemContext() {}

        public TestNotificationItemContext(string toAddress)
        {
            ToAddress = toAddress;
        }

        public string ToAddress { get; set; }
    }
}
