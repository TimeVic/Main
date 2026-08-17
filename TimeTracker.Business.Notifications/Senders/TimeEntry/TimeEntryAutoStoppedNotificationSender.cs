using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationSender : IAsyncQueueHandler<TimeEntryAutoStoppedNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITimeEntryDao _timeEntryDao;

        public TimeEntryAutoStoppedNotificationSender(
            ISmtpClientService smtpClientService,
            IEmailTemplateService emailTemplateService,
            ITimeEntryDao timeEntryDao
        )
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
            _timeEntryDao = timeEntryDao;
        }

        public async Task HandleAsync(
            TimeEntryAutoStoppedNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var timeEntry = await _timeEntryDao.GetByIdAsync(context.TimeEntryId);
            if (timeEntry == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("TimeEntryAutoStoppedNotification.htm", timeEntry.User);
            emailBuilder.AddPlaceholder("projectName", timeEntry.Project?.Name ?? "—");
            _smtpClientService.SendEmail(timeEntry.User.Email, emailBuilder, null);
        }
    }
}
