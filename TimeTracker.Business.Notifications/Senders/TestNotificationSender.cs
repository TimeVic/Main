using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders
{
    public class TestNotificationSender : IAsyncQueueHandler<TestNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;

        public TestNotificationSender(
            ISmtpClientService smtpClientService,
            IEmailTemplateService emailTemplateService
        )
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
        }

        public Task HandleAsync(
            TestNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailTemplateService.GetEmailBuilder("TestNotification.htm", "en");
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
