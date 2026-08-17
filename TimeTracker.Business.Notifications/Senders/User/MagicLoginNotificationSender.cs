using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.User;

public class MagicLoginNotificationSender : IAsyncQueueHandler<MagicLoginNotificationItemContext>
{
    private readonly ISmtpClientService _smtpClientService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUserMagicTokenDao _userMagicTokenDao;
    private readonly string _frontendUrl;

    public MagicLoginNotificationSender(
        ISmtpClientService smtpClientService,
        IEmailTemplateService emailTemplateService,
        IUserMagicTokenDao userMagicTokenDao,
        IConfiguration configuration
    )
    {
        _smtpClientService = smtpClientService;
        _emailTemplateService = emailTemplateService;
        _userMagicTokenDao = userMagicTokenDao;
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl")?.TrimEnd('/') ?? string.Empty;
    }

    public async Task HandleAsync(
        MagicLoginNotificationItemContext context,
        CancellationToken cancellationToken = default
    )
    {
        var magicToken = await _userMagicTokenDao.GetAsync(id: context.MagicTokenId);
        if (magicToken == null)
            return;

        var emailBuilder = _emailTemplateService.GetEmailBuilder("MagicLoginNotification.htm", magicToken.User);
        emailBuilder.AddPlaceholder("loginUrl", $"{_frontendUrl}/login/magic/{Uri.EscapeDataString(magicToken.Token)}");
        _smtpClientService.SendEmail(magicToken.User.Email, emailBuilder, null);
    }
}
