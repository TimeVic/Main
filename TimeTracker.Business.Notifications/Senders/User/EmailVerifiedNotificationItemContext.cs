using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationItemContext : INotificationItemContext
    {
        public string ToAddress { get; set; } = string.Empty;
        public string VerifiedEmail { get; set; } = string.Empty;
        
        public EmailVerifiedNotificationItemContext() {}

        public EmailVerifiedNotificationItemContext(string toAddress, string verifiedEmail)
        {
            ToAddress = toAddress;
            VerifiedEmail = verifiedEmail;
        }
    }
}
