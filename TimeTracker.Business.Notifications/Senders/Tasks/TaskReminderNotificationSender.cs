using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskReminderNotificationSender : IAsyncQueueHandler<TaskReminderNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITaskDao _taskDao;
        private readonly IUserDao _userDao;
        private readonly string? _frontendUrl;

        public TaskReminderNotificationSender(
            ISmtpClientService smtpClientService,
            IConfiguration configuration,
            IEmailTemplateService emailTemplateService,
            ITaskDao taskDao,
            IUserDao userDao
        )
        {
            _smtpClientService = smtpClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailTemplateService = emailTemplateService;
            _taskDao = taskDao;
            _userDao = userDao;
        }

        public async Task HandleAsync(
            TaskReminderNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            await SendEmailNotification(context);
        }

        private async Task SendEmailNotification(TaskReminderNotificationContext context)
        {
            var task = await _taskDao.GetById(context.TaskId);
            var recipient = await _userDao.GetById(context.RecipientUserId);
            if (task == null || recipient == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("TaskReminderNotification.htm", recipient);
            emailBuilder.AddPlaceholder("userName", task.User.Name);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{task.Workspace.Id}/task/{task.Id}");
            emailBuilder.AddPlaceholder("taskTitle", task.Title);
            _smtpClientService.SendEmail(recipient.Email, emailBuilder, null);
        }
    }
}
