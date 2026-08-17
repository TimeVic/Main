using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class ResetPasswordNotificationSender : IAsyncQueueHandler<ResetPasswordNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserResetPasswordRequestDao _resetPasswordRequestDao;
        private readonly string _frontendUrl;

        public ResetPasswordNotificationSender(
            ISmtpClientService smtpClientService,
            IEmailTemplateService emailTemplateService,
            IUserResetPasswordRequestDao resetPasswordRequestDao,
            IConfiguration configuration
        )
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
            _resetPasswordRequestDao = resetPasswordRequestDao;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl")?.TrimEnd('/') ?? string.Empty;
        }

        public async Task HandleAsync(
            ResetPasswordNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var resetPasswordRequest = await _resetPasswordRequestDao.GetAsync(id: context.ResetPasswordRequestId);
            if (resetPasswordRequest == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("ResetPasswordNotification.htm", resetPasswordRequest.User);
            emailBuilder.AddPlaceholder("resetPasswordUrl", $"{_frontendUrl}/user/change-password/{Uri.EscapeDataString(resetPasswordRequest.VerificationToken)}");
            _smtpClientService.SendEmail(resetPasswordRequest.User.Email, emailBuilder, null);
        }
    }
}
