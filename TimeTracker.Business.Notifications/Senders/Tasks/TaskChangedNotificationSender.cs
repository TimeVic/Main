using System.Web;
using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskChangedNotificationSender : IAsyncQueueHandler<TaskChangedNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly string? _frontendUrl;

        public TaskChangedNotificationSender(
            ISmtpClientService smtpClientService,
            IConfiguration configuration,
            IEmailTemplateService emailTemplateService
        )
        {
            _smtpClientService = smtpClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailTemplateService = emailTemplateService;
        }

        public async Task HandleAsync(
            TaskChangedNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("TaskChangedNotification.htm", context.ToAddress);
            emailBuilder.AddPlaceholder("userName", context.UserName);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{context.WorkspaceId}/task/{context.TaskId}");
            emailBuilder.AddPlaceholder("taskTitle", context.TaskTitle);
            emailBuilder.AddPlaceholder("changesBlock", BuildChangeSetBlock(context.ChangeSet));
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        }

        private string BuildChangeSetBlock(Dictionary<string, string?> changeSet)
        {
            var result = "<table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse:collapse;\">";
            foreach (var changeKeyPair in changeSet)
            {
                var changeString = string.IsNullOrEmpty(changeKeyPair.Key)
                    ? changeKeyPair.Value
                    : $"{changeKeyPair.Key}: <b>{changeKeyPair.Value}</b>";
                result += $"<tr><td style=\"padding:0 0 8px;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:15px;line-height:24px;color:#334155;\">{changeString}</td></tr>";
            }
            return $"{result}</table>";
        }
    }
}
