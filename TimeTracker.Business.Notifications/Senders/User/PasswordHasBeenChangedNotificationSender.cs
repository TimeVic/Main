using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class PasswordHasBeenChangedNotificationSender : IAsyncQueueHandler<PasswordHasBeenChangedNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserDao _userDao;

        public PasswordHasBeenChangedNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService, IUserDao userDao)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
            _userDao = userDao;
        }

        public async Task HandleAsync(
            PasswordHasBeenChangedNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userDao.GetById(context.UserId);
            if (user == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("PasswordHasBeenChangedNotification.htm", user);
            _smtpClientService.SendEmail(user.Email, emailBuilder, null);
        }
    }
}
