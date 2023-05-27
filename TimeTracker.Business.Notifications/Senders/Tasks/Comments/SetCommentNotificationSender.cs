using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Notifications.Core.Emails;
using TimeTracker.Business.Notifications.Services;

namespace TimeTracker.Business.Notifications.Senders.Tasks.Comments
{
    public class SetCommentNotificationSender : IAsyncNotification<SetCommentNotificationContext>
    {
        private readonly IEmailSendingService _emailSendingService;
        private readonly EmailFactory _emailFactory;
        private readonly string? _frontendUrl;

        public SetCommentNotificationSender(
            IEmailSendingService emailSendingService,
            IConfiguration configuration
        )
        {
            _emailSendingService = emailSendingService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailFactory = new EmailFactory();
        }

        public Task SendAsync(
            SetCommentNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = _emailFactory.GetEmailBuilder("TaskCommentSetNotification.htm");
            emailBuilder.AddPlaceholder("UserName", context.OwnerName);
            emailBuilder.AddPlaceholder("Comment", MarkdownHelper.ToHtml(context.Comment));
            emailBuilder.AddPlaceholder("TaskLink", $"{_frontendUrl}/board/task/" + context.TaskId);
            emailBuilder.AddPlaceholder("ChangeMessage", context.IsUpdated ? "updated" : "added");
            _emailSendingService.SendEmail(context.ToAddress, emailBuilder, null);
            return Task.CompletedTask;
        }
    }
}
