using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class ResetPasswordNotificationSender : IAsyncNotification<ResetPasswordNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly EmailFactory _emailFactory;

        public ResetPasswordNotificationSender(ISmtpClientService smtpClientService)
        {
            _smtpClientService = smtpClientService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            ResetPasswordNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("ResetPasswordNotification.htm");
            emailBuilder.AddPlaceholder("resetPasswordUrl", context.ResetPasswordUrl);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
