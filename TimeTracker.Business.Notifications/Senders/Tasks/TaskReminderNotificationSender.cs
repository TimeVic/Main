using System.Web;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskReminderNotificationSender : IAsyncNotification<TaskReminderNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IFirebaseClientService _firebaseClientService;
        private readonly EmailFactory _emailFactory;
        private readonly string? _frontendUrl;

        public TaskReminderNotificationSender(
            ISmtpClientService smtpClientService,
            IFirebaseClientService firebaseClientService,
            IConfiguration configuration
        )
        {
            _smtpClientService = smtpClientService;
            _firebaseClientService = firebaseClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailFactory = new EmailFactory();
        }

        public async Task SendAsync(
            TaskReminderNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            await SendEmailNotification(context);
        }

        private async Task SendEmailNotification(TaskReminderNotificationContext context)
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TaskReminderNotification.htm");
            emailBuilder.AddPlaceholder("userName", context.UserName);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl}/board/task/{context.WorkspaceId}/{context.TaskId}");
            emailBuilder.AddPlaceholder("taskTitle", context.TaskTitle);
            _smtpClientService.SendEmail(context.ToEmailAddress, emailBuilder, null);
        }
        
        private async Task SendGcmNotification(TaskReminderNotificationContext context)
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TaskReminderNotification.htm");
            emailBuilder.AddPlaceholder("userName", context.UserName);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl}/board/task/{context.WorkspaceId}/{context.TaskId}");
            emailBuilder.AddPlaceholder("taskTitle", context.TaskTitle);
            _smtpClientService.SendEmail(context.ToEmailAddress, emailBuilder, null);
        }
    }
}
