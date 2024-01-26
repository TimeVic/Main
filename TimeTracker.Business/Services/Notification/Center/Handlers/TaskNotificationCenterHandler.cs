using Domain.Abstractions;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Push;

namespace TimeTracker.Business.Services.Notification.Center.Handlers;

public class TaskNotificationCenterHandler: INotificationCenterHandler<TaskEntity>
{
    private readonly IGcmNotificationService _gcmNotificationService;
    private readonly IDbSessionProvider _sessionProvider;

    public TaskNotificationCenterHandler(
        IGcmNotificationService gcmNotificationService,
        IDbSessionProvider sessionProvider
    )
    {
        _gcmNotificationService = gcmNotificationService;
        _sessionProvider = sessionProvider;
    }

    public async Task Handle<T>(
        NotificationActionType action,
        UserEntity performedUser,
        T entity
    )
    {
        var task = (entity as TaskEntity)!;
        if (action == NotificationActionType.Reminder)
        {
            var notification = new NotificationEntity
            {
                Type = action,
                Workspace = task.Workspace,
                PerformedUser = performedUser,
                Task = task,
                ReceiverUser = task.User
            };
            await _gcmNotificationService.SendTaskReminderNotification(task);
            await _sessionProvider.CurrentSession.SaveAsync(notification);
        }
    }
}
