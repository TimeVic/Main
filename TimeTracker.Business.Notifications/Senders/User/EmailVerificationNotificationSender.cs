using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerificationNotificationSender : IAsyncQueueHandler<EmailVerificationNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserDao _userDao;
        private readonly string _frontendUrl;

        public EmailVerificationNotificationSender(
            ISmtpClientService smtpClientService,
            IEmailTemplateService emailTemplateService,
            IUserDao userDao,
            IConfiguration configuration
        )
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
            _userDao = userDao;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl")?.TrimEnd('/') ?? string.Empty;
        }

        public async Task HandleAsync(
            EmailVerificationNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userDao.GetById(context.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.VerificationToken))
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("EmailVerificationNotification.htm", user);
            emailBuilder.AddPlaceholder("verificationUrl", $"{_frontendUrl}/email/verification/{Uri.EscapeDataString(user.VerificationToken)}");
            _smtpClientService.SendEmail(user.Email, emailBuilder, null);
        }
    }
}
