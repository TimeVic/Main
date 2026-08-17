using System.Web;
using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskReminderNotificationSender : IAsyncQueueHandler<TaskReminderNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IFirebaseClientService _firebaseClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly string? _frontendUrl;

        public TaskReminderNotificationSender(
            ISmtpClientService smtpClientService,
            IFirebaseClientService firebaseClientService,
            IConfiguration configuration,
            IEmailTemplateService emailTemplateService
        )
        {
            _smtpClientService = smtpClientService;
            _firebaseClientService = firebaseClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailTemplateService = emailTemplateService;
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
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("TaskReminderNotification.htm", context.ToEmailAddress);
            emailBuilder.AddPlaceholder("userName", context.UserName);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{context.WorkspaceId}/task/{context.TaskId}");
            emailBuilder.AddPlaceholder("taskTitle", context.TaskTitle);
            _smtpClientService.SendEmail(context.ToEmailAddress, emailBuilder, null);
        }
        
        private async Task SendGcmNotification(TaskReminderNotificationContext context)
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("TaskReminderNotification.htm", context.ToEmailAddress);
            emailBuilder.AddPlaceholder("userName", context.UserName);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{context.WorkspaceId}/task/{context.TaskId}");
            emailBuilder.AddPlaceholder("taskTitle", context.TaskTitle);
            _smtpClientService.SendEmail(context.ToEmailAddress, emailBuilder, null);
        }
    }
}
