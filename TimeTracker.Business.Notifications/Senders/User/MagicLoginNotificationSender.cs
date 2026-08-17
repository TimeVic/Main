using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.User;

public class MagicLoginNotificationSender : IAsyncQueueHandler<MagicLoginNotificationItemContext>
{
    private readonly ISmtpClientService _smtpClientService;
    private readonly IEmailTemplateService _emailTemplateService;

    public MagicLoginNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService)
    {
        _smtpClientService = smtpClientService;
        _emailTemplateService = emailTemplateService;
    }

    public async Task HandleAsync(
        MagicLoginNotificationItemContext context,
        CancellationToken cancellationToken = default
    )
    {
        var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("MagicLoginNotification.htm", context.ToAddress);
        emailBuilder.AddPlaceholder("loginUrl", context.LoginUrl);
        _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
    }
}
