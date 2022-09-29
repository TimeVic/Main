using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationSender : IAsyncNotification<EmailVerifiedNotificationItemContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;

        public EmailVerifiedNotificationSender(IEmailSendingService emailSendingService)
        {
            _emailSendingService = emailSendingService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            EmailVerifiedNotificationItemContext commandItemContext, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("UserEmailVerifiedNotification.htm");
            emailBuilder.AddPlaceholder("email", commandItemContext.VerifiedEmail);
            _emailSendingService.SendEmail(commandItemContext.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
