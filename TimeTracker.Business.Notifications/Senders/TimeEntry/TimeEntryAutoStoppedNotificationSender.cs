using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationSender : IAsyncNotification<TimeEntryAutoStoppedNotificationItemContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;

        public TimeEntryAutoStoppedNotificationSender(IEmailSendingService emailSendingService)
        {
            _emailSendingService = emailSendingService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            TimeEntryAutoStoppedNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TimeEntryAutoStoppedNotification.htm");
            _emailSendingService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
