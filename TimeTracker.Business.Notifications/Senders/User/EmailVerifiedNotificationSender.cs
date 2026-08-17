using Domain.Abstractions;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.User
{
    public class EmailVerifiedNotificationSender : IAsyncQueueHandler<EmailVerifiedNotificationItemContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IUserDao _userDao;

        public EmailVerifiedNotificationSender(ISmtpClientService smtpClientService, IEmailTemplateService emailTemplateService, IUserDao userDao)
        {
            _smtpClientService = smtpClientService;
            _emailTemplateService = emailTemplateService;
            _userDao = userDao;
        }

        public async Task HandleAsync(
            EmailVerifiedNotificationItemContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userDao.GetById(context.UserId);
            if (user == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("UserEmailVerifiedNotification.htm", user);
            emailBuilder.AddPlaceholder("email", user.Email);
            _smtpClientService.SendEmail(user.Email, emailBuilder, null);
        }
    }
}
