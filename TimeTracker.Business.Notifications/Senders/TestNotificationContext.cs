using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders
{
    public class TestNotificationContext : INotificationContext
    {
        public TestNotificationContext() {}

        public TestNotificationContext(string toAddress)
        {
            ToAddress = toAddress;
        }

        public string ToAddress { get; set; }
    }
}
