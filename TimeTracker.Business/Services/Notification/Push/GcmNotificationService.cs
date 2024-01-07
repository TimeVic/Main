using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Notification.Push;

public class GcmNotificationService: IGcmNotificationService
{
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly IFirebaseClientService _firebaseClient;

    public GcmNotificationService(
        IUserNotificationTokenDao userNotificationTokenDao,
        IFirebaseClientService firebaseClient
    )
    {
        _userNotificationTokenDao = userNotificationTokenDao;
        _firebaseClient = firebaseClient;
    }

    public async Task SendTaskReminderNotification(TaskEntity task)
    {
        if (!task.RemindTime.HasValue)
            throw new NullReferenceException("Task should contain RemindedAt time");
        
        foreach (var notificationToken in task.User.NotificationTokens)
        {
            var isSent = await _firebaseClient.SendMessage(
                notificationToken.Token,
                $"Reminder: {task.Title.TruncateAndAddDots(30)}",
                $"Today {task.RemindTime:t}"
            );
            if (!isSent)
            {
                await _userNotificationTokenDao.Delete(notificationToken);
            }
        }
        task.RemindedTime = task.RemindTime;
    }
}
