namespace TimeTracker.Business.Services.Notification;

public partial class TaskNotificationService
{
    public async Task<bool> SendReminderNotification()
    {
        var tasks = await _taskDao.GetTasksToRemind();
        if (!tasks.Any())
        {
            return false;
        }
        foreach (var task in tasks)
        {
            await _notificationService.SendTaskReminderNotification(task);
        }
        await _sessionProvider.PerformCommitAsync();
        return true;
    }
}
