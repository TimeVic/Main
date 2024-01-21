using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Notification.Push;

public class GcmNotificationService: IGcmNotificationService
{
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IFirebaseClientService _firebaseClient;

    public GcmNotificationService(
        IUserNotificationTokenDao userNotificationTokenDao,
        IDbSessionProvider sessionProvider,
        IFirebaseClientService firebaseClient
    )
    {
        _userNotificationTokenDao = userNotificationTokenDao;
        _sessionProvider = sessionProvider;
        _firebaseClient = firebaseClient;
    }

    public async Task SendTaskReminderNotification(TaskEntity task)
    {
        if (!task.ReminderTime.HasValue)
            throw new NullReferenceException("Task should contain ReminderTime time");
        
        foreach (var notificationToken in task.User.NotificationTokens)
        {
            var isSent = await _firebaseClient.SendMessage(
                notificationToken.Token,
                $"Reminder: {task.Title.TruncateAndAddDots(30)}",
                $"Today {task.ReminderTime:t}"
            );
            if (!isSent)
            {
                await _userNotificationTokenDao.Delete(notificationToken);
            }
        }
        task.RemindedTime = task.ReminderTime;
    }
}
