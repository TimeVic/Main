using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Orm.Dao.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Business.Services.Notification;

public partial class NotificationBgJobService
{
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

    private async Task SendTaskUpdatedNotification(TaskHistoryItemEntity historyItem)
    {
        var receivers = new List<UserEntity>()
        {
            historyItem.AssigneeUser,
            historyItem.Task.Workspace.CreatedUser
        };
        foreach (var receiver in receivers.DistinctBy(item => item.Id))
        {
            await _queueService.PushNotificationAsync(new TaskChangedNotificationContext()
            {
                TaskHistoryItemId = historyItem.Id,
                RecipientUserId = receiver.Id
            });    
        }
    }
}
