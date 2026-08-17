using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class RegistrationNotificationSender : IAsyncQueueHandler<RegistrationNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserDao _userDao;
        private readonly string _frontendUrl;

        public RegistrationNotificationSender(
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
            RegistrationNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userDao.GetById(context.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.VerificationToken))
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("RegistrationNotification.htm", user);
            emailBuilder.AddPlaceholder("verificationUrl", $"{_frontendUrl}/registration/verification/{Uri.EscapeDataString(user.VerificationToken)}");
            _smtpClientService.SendEmail(user.Email, emailBuilder, null);
        }
    }
}
