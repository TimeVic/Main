using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class RegistrationNotificationSender : IAsyncNotification<RegistrationNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly EmailFactory _emailFactory;

        public RegistrationNotificationSender(ISmtpClientService smtpClientService)
        {
            _smtpClientService = smtpClientService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            RegistrationNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("RegistrationNotification.htm");
            emailBuilder.AddPlaceholder("verificationUrl", context.VerificationUrl);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
