using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Notifications.Core;

namespace TimeTracker.Business.Notifications.Senders.Tasks.Comments
{
    public class SetCommentNotificationSender : IAsyncQueueHandler<SetCommentNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly EmailFactory _emailFactory;
        private readonly string? _frontendUrl;

        public SetCommentNotificationSender(
            ISmtpClientService smtpClientService,
            IConfiguration configuration
        )
        {
            _smtpClientService = smtpClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailFactory = new EmailFactory();
        }

        public Task HandleAsync(
            SetCommentNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TaskCommentSetNotification.htm");
            emailBuilder.AddPlaceholder("UserName", context.OwnerName);
            emailBuilder.AddPlaceholder("Comment", MarkdownHelper.ToHtml(context.Comment));
            emailBuilder.AddPlaceholder("TaskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{context.WorkspaceId}/task/{context.TaskId}");
            emailBuilder.AddPlaceholder("ChangeMessage", context.IsUpdated ? "updated" : "added");
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
