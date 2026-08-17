using System.Web;
using Domain.Abstractions;
using Microsoft.Extensions.Configuration;
using Notification.Abstractions;
using TimeTracker.Business.Clients.Smtp;
using TimeTracker.Business.Clients.Smtp.Core;
using TimeTracker.Business.Notifications.Core;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Business.Notifications.Senders.Tasks
{
    public class TaskChangedNotificationSender : IAsyncQueueHandler<TaskChangedNotificationContext>
    {
        private readonly ISmtpClientService _smtpClientService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITaskHistoryItemDao _taskHistoryItemDao;
        private readonly IUserDao _userDao;
        private readonly string? _frontendUrl;

        public TaskChangedNotificationSender(
            ISmtpClientService smtpClientService,
            IConfiguration configuration,
            IEmailTemplateService emailTemplateService,
            ITaskHistoryItemDao taskHistoryItemDao,
            IUserDao userDao
        )
        {
            _smtpClientService = smtpClientService;
            _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
            _emailTemplateService = emailTemplateService;
            _taskHistoryItemDao = taskHistoryItemDao;
            _userDao = userDao;
        }

        public async Task HandleAsync(
            TaskChangedNotificationContext context, 
            CancellationToken cancellationToken = default
        )
        {
            var taskHistoryItem = await _taskHistoryItemDao.GetByIdAsync(context.TaskHistoryItemId);
            var recipient = await _userDao.GetById(context.RecipientUserId);
            if (taskHistoryItem == null || recipient == null)
                return;

            var emailBuilder = _emailTemplateService.GetEmailBuilder("TaskChangedNotification.htm", recipient);
            emailBuilder.AddPlaceholder("userName", taskHistoryItem.User.Name);
            emailBuilder.AddPlaceholder("taskLink", $"{_frontendUrl?.TrimEnd('/')}/board/{taskHistoryItem.Task.Workspace.Id}/task/{taskHistoryItem.Task.Id}");
            emailBuilder.AddPlaceholder("taskTitle", taskHistoryItem.Task.Title);
            emailBuilder.AddPlaceholder("changesBlock", BuildChangeSetBlock(BuildChangeSet(taskHistoryItem)));
            _smtpClientService.SendEmail(recipient.Email, emailBuilder, null);
        }

        private static Dictionary<string, string?> BuildChangeSet(TaskHistoryItemEntity historyItem)
        {
            var result = new Dictionary<string, string?>();
            var task = historyItem.Task;
            if (historyItem.Title != task.Title)
                result.Add("New title", task.Title);
            if (historyItem.Description != task.Description)
                result.Add("New description", task.Description);
            if (historyItem.Tags != task.TagsString)
                result.Add("New tags", task.TagsString);
            if (historyItem.Attachments != task.AttachmentsString && !string.IsNullOrEmpty(historyItem.Attachments) && !string.IsNullOrEmpty(task.AttachmentsString))
                result.Add("", "Added new attachments");
            if (historyItem.StartTime != task.StartTime)
                result.Add("New start time", historyItem.StartTime?.ToString() ?? "Not set");
            if (historyItem.EndTime != task.EndTime)
                result.Add("New end time", historyItem.EndTime?.ToString() ?? "Not set");
            if (historyItem.Status != task.Status)
                result.Add("Status changed", $"{historyItem.Status.GetDisplayName()} -> {task.Status.GetDisplayName()}");
            if (historyItem.Priority != task.Priority)
                result.Add("Priority changed", $"{historyItem.Priority.GetDisplayName()} -> {task.Priority.GetDisplayName()}");
            if (historyItem.IsArchived != task.IsArchived && historyItem.IsArchived)
                result.Add("", "Archived the task");
            if (historyItem.AssigneeUser.Id != task.User.Id)
                result.Add("Assigned to", task.User.Name);
            if (historyItem.TaskList.Id != task.TaskList.Id)
                result.Add("New task list", task.TaskList.Name);

            return result;
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
