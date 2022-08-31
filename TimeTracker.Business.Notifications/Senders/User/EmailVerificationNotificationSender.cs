using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerificationNotificationSender : IAsyncNotification<EmailVerificationNotificationContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;

        public EmailVerificationNotificationSender(IEmailSendingService emailSendingService)
        {
            _emailSendingService = emailSendingService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            EmailVerificationNotificationContext commandContext, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("EmailVerificationNotification.htm");
            emailBuilder.AddPlaceholder("verificationUrl", commandContext.VerificationUrl);
            _emailSendingService.SendEmail(commandContext.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
