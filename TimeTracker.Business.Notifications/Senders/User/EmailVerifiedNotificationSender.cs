using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationSender : IAsyncQueueHandler<EmailVerifiedNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;

        public EmailVerifiedNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task HandleAsync(
            EmailVerifiedNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("UserEmailVerifiedNotification.htm", context.ToAddress);
            emailBuilder.AddPlaceholder("email", context.VerifiedEmail);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        }
    }
}
