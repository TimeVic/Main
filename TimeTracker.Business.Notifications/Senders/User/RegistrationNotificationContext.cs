using System.Net;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class RegistrationNotificationContext : INotificationContext
    {
        public string ToAddress { get; set; }
        public string FrontendUrl { get; set; }
        public string VerificationToken { get; set; }
        public string VerificationUrl { get; set; }
        
        public RegistrationNotificationContext() {}

        public RegistrationNotificationContext(
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
