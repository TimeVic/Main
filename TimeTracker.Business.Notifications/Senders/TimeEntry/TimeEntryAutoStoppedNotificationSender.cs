using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.TimeEntry
{
    public class TimeEntryAutoStoppedNotificationSender : IAsyncQueueHandler<TimeEntryAutoStoppedNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserDao _userDao;

        public TimeEntryAutoStoppedNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService, IUserDao userDao)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
            _userDao = userDao;
        }

        public async Task HandleAsync(
            TimeEntryAutoStoppedNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userDao.GetById(context.UserId);
            if (user == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("TimeEntryAutoStoppedNotification.htm", user);
            _smtpClientService.SendEmail(user.Email, emailBuilder, null);
        }
    }
}
