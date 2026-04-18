using System.Net;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class RegistrationNotificationItemContext : INotificationItemContext
    {
        public string ToAddress { get; set; } = string.Empty;
        public string FrontendUrl { get; set; } = string.Empty;
        public string VerificationToken { get; set; } = string.Empty;
        public string VerificationUrl { get; set; } = string.Empty;
        
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
