using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class PasswordHasBeenChangedNotificationSender : IAsyncQueueHandler<PasswordHasBeenChangedNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;

        public PasswordHasBeenChangedNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task HandleAsync(
            PasswordHasBeenChangedNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("PasswordHasBeenChangedNotification.htm", context.ToAddress);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        }
    }
}
