using Notification.Abstractions;

namespace TimeTracker.Business.Notifications.Senders.User;

public class MagicLoginNotificationItemContext : INotificationItemContext
{
    public Guid MagicTokenId { get; set; }

    public MagicLoginNotificationItemContext() {}

    public MagicLoginNotificationItemContext(Guid magicTokenId)
    {
        MagicTokenId = magicTokenId;
    }
}
