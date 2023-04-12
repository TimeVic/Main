using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class RegistrationNotificationSender : IAsyncNotification<RegistrationNotificationItemContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;

        public RegistrationNotificationSender(IEmailSendingService emailSendingService)
        {
            _emailSendingService = emailSendingService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            RegistrationNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("RegistrationNotification.htm");
            emailBuilder.AddPlaceholder("verificationUrl", context.VerificationUrl);
            _emailSendingService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
