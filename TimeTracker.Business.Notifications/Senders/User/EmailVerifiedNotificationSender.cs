using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationSender : IAsyncNotification<EmailVerifiedNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly EmailFactory _emailFactory;

        public EmailVerifiedNotificationSender(ISmtpClientService smtpClientService)
        {
            _smtpClientService = smtpClientService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            EmailVerifiedNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("UserEmailVerifiedNotification.htm");
            emailBuilder.AddPlaceholder("email", context.VerifiedEmail);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
