using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.User;

public class MagicLoginNotificationSender : IAsyncQueueHandler<MagicLoginNotificationItemContext>
{
    private readonly ISmtpClientService _smtpClientService;
    private readonly EmailFactory _emailFactory;

    public MagicLoginNotificationSender(ISmtpClientService smtpClientService)
    {
        _smtpClientService = smtpClientService;
        _emailFactory = new EmailFactory();
    }

    public Task HandleAsync(
        MagicLoginNotificationItemContext context,
        CancellationToken cancellationToken = default
    )
    {
        var emailBuilder = _emailFactory.GetEmailBuilder("MagicLoginNotification.htm");
        emailBuilder.AddPlaceholder("loginUrl", context.LoginUrl);
        _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        return Task.CompletedTask;
    }
}
