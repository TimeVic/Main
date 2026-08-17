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
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly string? _frontendUrl;

        public SetCommentNotificationSender(
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
            SetCommentNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var emailBuilder = await _emailTemplateService.GetEmailBuilderAsync("TaskCommentSetNotification.htm", context.ToAddress);
            emailBuilder.AddPlaceholder("UserName", context.OwnerName);
            emailBuilder.AddPlaceholder("Comment", MarkdownHelper.ToHtml(context.Comment));
            emailBuilder.AddPlaceholder("TaskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{context.WorkspaceId}/task/{context.TaskId}");
            emailBuilder.AddPlaceholder("ChangeMessage", context.IsUpdated ? "updated" : "added");
            _smtpClientService.SendEmail(context.ToAddress, emailBuilder, null);
        }
    }
}
