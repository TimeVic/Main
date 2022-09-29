using System.Net;
using System.Web;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerificationNotificationItemContext : INotificationItemContext
    {
        public string ToAddress { get; set; }
        public string FrontendUrl { get; set; }
        public string VerificationToken { get; set; }
        public string VerificationUrl { get; set; }

        public EmailVerificationNotificationItemContext() {}

        public EmailVerificationNotificationItemContext(
            string toAddress, 
            string frontendUrl,
            string verificationToken    
        )
        {
            ToAddress = toAddress;
            FrontendUrl = frontendUrl;
            VerificationToken = WebUtility.UrlEncode(verificationToken);
            VerificationUrl = $"{FrontendUrl}/email/verification/" + HttpUtility.UrlPathEncode(VerificationToken);
        }
    }
}
