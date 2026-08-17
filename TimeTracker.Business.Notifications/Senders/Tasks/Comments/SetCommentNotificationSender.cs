using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Business.Notifications.Senders.Tasks.Comments
{
    public class SetCommentNotificationSender : IAsyncQueueHandler<SetCommentNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITaskCommentDao _taskCommentDao;
        private readonly IUserDao _userDao;
        private readonly string? _frontendUrl;

        public SetCommentNotificationSender(
            ISmtpClientService smtpClientService,
            IConfiguration configuration,
            IEmailTemplateService emailTemplateService,
            ITaskCommentDao taskCommentDao,
            IUserDao userDao
        )
        {
            _smtpClientService = smtpClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailTemplateService = emailTemplateService;
            _taskCommentDao = taskCommentDao;
            _userDao = userDao;
        }

        public async Task HandleAsync(
            SetCommentNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var taskComment = await _taskCommentDao.GetById(context.TaskCommentId);
            var recipient = await _userDao.GetById(context.RecipientUserId);
            if (taskComment?.User == null || recipient == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("TaskCommentSetNotification.htm", recipient);
            emailBuilder.AddPlaceholder("UserName", taskComment.User.Name);
            emailBuilder.AddPlaceholder("Comment", MarkdownHelper.ToHtml(taskComment.Comment));
            emailBuilder.AddPlaceholder("TaskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{taskComment.Task.Workspace.Id}/task/{taskComment.Task.Id}");
            _smtpClientService.SendEmail(recipient.Email, emailBuilder, null);
        }
    }
}
