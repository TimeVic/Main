using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Notification;

public class TaskNotificationService: ITaskNotificationService
{
    private readonly ITaskHistoryItemDao _taskHistoryItemDao;
    private readonly IQueueService _queueService;

    public TaskNotificationService(
        ITaskHistoryItemDao taskHistoryItemDao,
        IQueueService queueService
    )
    {
        _taskHistoryItemDao = taskHistoryItemDao;
        _queueService = queueService;
    }

    public async Task<bool> NotifyAboutTaskChanges()
    {
        var historyItemsToNotify = await _taskHistoryItemDao.GetBatchToNotify();
        if (!historyItemsToNotify.Any())
        {
            return false;
        }

        foreach (var itemToNotify in historyItemsToNotify)
        {
            await SendTaskUpdatedNotification(itemToNotify);
        }
        return true;
    }

    private Dictionary<string, string?> BuildChangeSet(TaskHistoryItemEntity historyItem)
    {
        var result = new Dictionary<string, string?>();
        var task = historyItem.Task;
        if (historyItem.Title != task.Title)
            result.Add("New title:", task.Title);
        if (historyItem.Description != task.Description)
            result.Add("New description:", task.Description);
        if (historyItem.Tags != task.TagsString)
            result.Add("New tags:", task.TagsString);
        if (historyItem.Attachments != task.AttachmentsString)
            result.Add("Added new attachments", "");
        if (historyItem.NotificationTime != task.NotificationTime)
            result.Add("New notification time:", "");
        if (historyItem.IsDone != task.IsDone)
            result.Add("Marked task as:", task.IsDone ? "Done" : "Active");
        if (historyItem.IsArchived != task.IsArchived && historyItem.IsArchived)
            result.Add("Archived the task", "");
        if (historyItem.AssigneeUser.Id != task.User.Id)
            result.Add("Assigned to:", task.User.Name);
        if (historyItem.TaskList.Id != task.TaskList.Id)
            result.Add("New task list:", task.TaskList.Name);

        return result;
    }

    private async Task SendTaskUpdatedNotification(TaskHistoryItemEntity historyItem)
    {
        var changeSet = BuildChangeSet(historyItem);
        if (!changeSet.Any())
        {
            return;
        }
        var receivers = new List<string>()
        {
            historyItem.AssigneeUser.Email,
            historyItem.Task.Workspace.CreatedUser.Email
        };
        foreach (var receiverEmail in receivers)
        {
            await _queueService.PushNotificationAsync(new TaskChangedNotificationContext()
            {
                UserName = historyItem.User.Name,
                ToAddress = receiverEmail,
                TaskId = historyItem.Task.Id,
                TaskTitle = historyItem.Task.Title,
                ChangeSet = changeSet
            });    
        }
    }
}
