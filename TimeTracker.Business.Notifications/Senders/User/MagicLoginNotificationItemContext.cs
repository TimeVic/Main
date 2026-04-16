using System.Net;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User;

public class MagicLoginNotificationItemContext : INotificationItemContext
{
    public string ToAddress { get; set; }
    public string FrontendUrl { get; set; }
    public string Token { get; set; }
    public string LoginUrl { get; set; }

    public MagicLoginNotificationItemContext() {}

    public MagicLoginNotificationItemContext(
        string toAddress,
        string frontendUrl,
        string token
    )
    {
        ToAddress = toAddress;
        FrontendUrl = frontendUrl;
        Token = WebUtility.UrlEncode(token);
        LoginUrl = $"{FrontendUrl}/login/magic/{Token}";
    }
}
