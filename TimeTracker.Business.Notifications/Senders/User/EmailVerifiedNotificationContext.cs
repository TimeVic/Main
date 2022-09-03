using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationContext : INotificationContext
    {
        public string ToAddress { get; set; }
        public string VerifiedEmail { get; set; }
        
        public EmailVerifiedNotificationContext() {}

        public EmailVerifiedNotificationContext(string toAddress, string verifiedEmail)
        {
            ToAddress = toAddress;
            VerifiedEmail = verifiedEmail;
        }
    }
}
