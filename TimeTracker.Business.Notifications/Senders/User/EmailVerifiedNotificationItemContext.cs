using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationItemContext : INotificationItemContext
    {
        public string ToAddress { get; set; }
        public string VerifiedEmail { get; set; }
        
        public EmailVerifiedNotificationItemContext() {}

        public EmailVerifiedNotificationItemContext(string toAddress, string verifiedEmail)
        {
            ToAddress = toAddress;
            VerifiedEmail = verifiedEmail;
        }
    }
}
