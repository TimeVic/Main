using TimeTracker.Business.Notifications.Senders.Tasks;
using TimeTracker.Business.Orm.Entities.Tasks;

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
            await SendReminder(task);
        }
        return true;
    }

    private async Task SendReminder(TaskEntity task)
    {
        await _queueService.PushNotificationAsync(new TaskReminderNotificationContext()
        {
            ToEmailAddress = task.User.Email,
            UserName = task.User.Name,
            GcmTokens = task.User.NotificationTokens.Select(item => item.Token).ToList(),
            TaskId = task.TaskId,
            WorkspaceId = task.TaskList.Project.Workspace.Id,
            TaskTitle = task.Title
        });
    }
}
