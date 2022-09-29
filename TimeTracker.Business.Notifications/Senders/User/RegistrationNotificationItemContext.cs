using System.Net;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class RegistrationNotificationItemContext : INotificationItemContext
    {
        public string ToAddress { get; set; }
        public string FrontendUrl { get; set; }
        public string VerificationToken { get; set; }
        public string VerificationUrl { get; set; }
        
        public RegistrationNotificationItemContext() {}

        public RegistrationNotificationItemContext(
            string toAddress, 
            string frontendUrl,
            string verificationToken
        )
        {
            ToAddress = toAddress;
            FrontendUrl = frontendUrl;
            VerificationToken = WebUtility.UrlEncode(verificationToken);
            VerificationUrl = $"{FrontendUrl}/registration/verification/{VerificationToken}";
        }
    }
}
