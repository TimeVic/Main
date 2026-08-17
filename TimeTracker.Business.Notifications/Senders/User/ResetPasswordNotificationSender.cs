using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class ResetPasswordNotificationSender : IAsyncQueueHandler<ResetPasswordNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;

        public ResetPasswordNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task HandleAsync(
            ResetPasswordNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("ResetPasswordNotification.htm", context.ToAddress);
            emailBuilder.AddPlaceholder("resetPasswordUrl", context.ResetPasswordUrl);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        }
    }
}
