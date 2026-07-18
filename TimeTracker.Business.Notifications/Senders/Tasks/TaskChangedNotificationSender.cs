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
        private readonly EmailFactory _emailFactory;
        private readonly string? _frontendUrl;

        public TaskChangedNotificationSender(
            ISmtpClientService smtpClientService,
            IConfiguration configuration
        )
        {
            _smtpClientService = smtpClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailFactory = new EmailFactory();
        }

        public Task HandleAsync(
            TaskChangedNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TaskChangedNotification.htm");
            emailBuilder.AddPlaceholder("userName", context.UserName);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{context.WorkspaceId}/task/{context.TaskId}");
            emailBuilder.AddPlaceholder("taskTitle", context.TaskTitle);
            emailBuilder.AddPlaceholder("changesBlock", BuildChangeSetBlock(context.ChangeSet));
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }

        private string BuildChangeSetBlock(Dictionary<string, string?> changeSet)
        {
            var result = "<div>";
            foreach (var changeKeyPair in changeSet)
            {
                var changeString = string.IsNullOrEmpty(changeKeyPair.Key)
                    ? changeKeyPair.Value
                    : $"{changeKeyPair.Key}: <b>{changeKeyPair.Value}</b>";
                result += $"<p>{changeString}</p>";
            }
            return $"{result}</dev>";
        }
    }
}
