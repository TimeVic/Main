using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerificationNotificationSender : IAsyncNotification<EmailVerificationNotificationItemContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;

        public EmailVerificationNotificationSender(IEmailSendingService emailSendingService)
        {
            _emailSendingService = emailSendingService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            EmailVerificationNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("EmailVerificationNotification.htm");
            emailBuilder.AddPlaceholder("verificationUrl", context.VerificationUrl);
            _emailSendingService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
