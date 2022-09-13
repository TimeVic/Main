using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationSender : IAsyncNotification<TimeEntryAutoStoppedNotificationContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;

        public TimeEntryAutoStoppedNotificationSender(IEmailSendingService emailSendingService)
        {
            _emailSendingService = emailSendingService;
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            TimeEntryAutoStoppedNotificationContext commandContext, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TimeEntryAutoStoppedNotification.htm");
            _emailSendingService.SendEmail(commandContext.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
