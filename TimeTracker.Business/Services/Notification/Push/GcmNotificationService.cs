using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Clients.Api;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Services.Notification.Push;

public class GcmNotificationService: IGcmNotificationService
{
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IFirebaseClientService _firebaseClient;
    private readonly IUrlService _urlService;

    public GcmNotificationService(
        IUserNotificationTokenDao userNotificationTokenDao,
        IDbSessionProvider sessionProvider,
        IFirebaseClientService firebaseClient,
        IUrlService urlService
    )
    {
        _userNotificationTokenDao = userNotificationTokenDao;
        _sessionProvider = sessionProvider;
        _firebaseClient = firebaseClient;
        _urlService = urlService;
    }

    private async Task SendGcmNotification(
        ICollection<UserNotificationTokenEntity> tokens,
        string title,
        string body,
        string? link = null
    )
    {
        foreach (var token in tokens)
        {
            var isSent = await _firebaseClient.SendMessage(
                token.Token,
                title,
                body,
                link
            );
            if (!isSent)
            {
                await _userNotificationTokenDao.Delete(token);
            }
        }
    }

    public async Task SendTaskReminderNotification(TaskEntity task)
    {
        if (!task.ReminderTime.HasValue)
            throw new NullReferenceException("Task should contain ReminderTime time");

        await SendGcmNotification(
            task.User.NotificationTokens,
            $"Reminder: {task.Title.TruncateAndAddDots(30)}",
            $"Today {task.ReminderTime:t}"
        );
        task.RemindedTime = task.ReminderTime;
    }
    
    public async Task SendTaskCommentNotification(
        TaskCommentEntity comment,
        UserEntity receiverUser,
        UserEntity performedUsed,
        bool isChanged
    )
    {
        await SendGcmNotification(
            receiverUser.NotificationTokens,
            performedUsed.Name + (isChanged ? " changed" : " added") + " comment:",
            comment.Comment.Truncate(200, true)
        );
    }

    public async Task SendTimeEntryRunningTooLongNotification(TimeEntryEntity timeEntry)
    {
        await SendGcmNotification(
            timeEntry.User.NotificationTokens,
            "TimeVic timer reminder",
            "Your timer has been running for more than 10 hours.",
            _urlService.ToFrontendAbsoluteUrl($"/board/{timeEntry.Workspace.Id}")
        );
    }
}
