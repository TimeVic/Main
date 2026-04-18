using System.Net;
using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User;

public class MagicLoginNotificationItemContext : INotificationItemContext
{
    public string ToAddress { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string LoginUrl { get; set; } = string.Empty;

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
