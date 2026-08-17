using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerificationNotificationSender : IAsyncQueueHandler<EmailVerificationNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;

        public EmailVerificationNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task HandleAsync(
            EmailVerificationNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("EmailVerificationNotification.htm", context.ToAddress);
            emailBuilder.AddPlaceholder("verificationUrl", context.VerificationUrl);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        }
    }
}
