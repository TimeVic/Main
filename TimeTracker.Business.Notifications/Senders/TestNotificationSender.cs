using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders
{
    public class TestNotificationSender : IAsyncNotification<TestNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly EmailFactory _emailFactory;

        public TestNotificationSender(
            ISmtpClientService smtpClientService
        )
        {
            _smtpClientService = smtpClientService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            TestNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TestNotification.htm");
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
