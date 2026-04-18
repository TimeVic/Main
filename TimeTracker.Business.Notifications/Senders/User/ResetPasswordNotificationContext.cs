using System.Net;
using System.Web;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class ResetPasswordNotificationContext : INotificationItemContext
    {
        public string ToAddress { get; set; } = string.Empty;
        public string FrontendUrl { get; set; } = string.Empty;
        public string VerificationToken { get; set; } = string.Empty;

        public string ResetPasswordUrl
        {
            get => $"{FrontendUrl}/user/change-password/" + HttpUtility.UrlPathEncode(VerificationToken);
        }

        public ResetPasswordNotificationContext() {}
    }
}
