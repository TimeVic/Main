using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders
{
    public class TestNotificationSender : IAsyncNotification<TestNotificationContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;

        public TestNotificationSender(
            IEmailSendingService emailSendingService
        )
        {
            _emailSendingService = emailSendingService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            TestNotificationContext commandContext, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TestNotification.htm");
            _emailSendingService.SendEmail(commandContext.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
