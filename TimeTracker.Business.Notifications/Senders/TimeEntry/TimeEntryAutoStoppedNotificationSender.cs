using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationSender : IAsyncQueueHandler<TimeEntryAutoStoppedNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;

        public TimeEntryAutoStoppedNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task HandleAsync(
            TimeEntryAutoStoppedNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("TimeEntryAutoStoppedNotification.htm", context.ToAddress);
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        }
    }
}
