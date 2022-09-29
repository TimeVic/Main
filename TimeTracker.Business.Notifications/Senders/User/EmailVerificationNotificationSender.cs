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
            EmailVerificationNotificationItemContext commandItemContext, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("EmailVerificationNotification.htm");
            emailBuilder.AddPlaceholder("verificationUrl", commandItemContext.VerificationUrl);
            _emailSendingService.SendEmail(commandItemContext.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
