using System.Web;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskChangedNotificationSender : IAsyncNotification<TaskChangedNotificationContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;
        private readonly string? _frontendUrl;

        public TaskChangedNotificationSender(
            IEmailSendingService emailSendingService,
            IConfiguration configuration
        )
        {
            _emailSendingService = emailSendingService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            TaskChangedNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TaskChangedNotification.htm");
            emailBuilder.AddPlaceholder("userName", context.UserName);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl}/board/task/" + context.TaskId);
            emailBuilder.AddPlaceholder("taskTitle", context.UserName);
            emailBuilder.AddPlaceholder("changesBlock", BuildChangeSetBlock(context.ChangeSet));
            _emailSendingService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }

        private string BuildChangeSetBlock(Dictionary<string, string?> changeSet)
        {
            var result = "<div>";
            foreach (var changeKeyPair in changeSet)
            {
                var changeString = string.IsNullOrEmpty(changeKeyPair.Value)
                    ? changeKeyPair.Key
                    : $"{changeKeyPair.Key}: <b>{changeKeyPair.Value}</b>";
                result += $"<p>{changeString}</p>";
            }
            return $"{result}</dev>";
        }
    }
}
