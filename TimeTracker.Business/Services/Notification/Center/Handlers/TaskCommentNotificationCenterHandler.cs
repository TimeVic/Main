using Domain.Abstractions;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Push;

namespace TimeTracker.Business.Services.Notification.Center.Handlers;

public class TaskCommentNotificationCenterHandler: INotificationCenterHandler<TaskCommentEntity>
{
    private readonly IGcmNotificationService _gcmNotificationService;
    private readonly IDbSessionProvider _sessionProvider;

    public TaskCommentNotificationCenterHandler(
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
        var comment = (entity as TaskCommentEntity)!;
        if (action is NotificationActionType.EditEntity or NotificationActionType.AddEntity)
        {
            var receivers = new List<UserEntity>()
            {
                comment.User
            };
            receivers = receivers.Concat(comment.Watchers)
                .Where(item => item.Id != performedUser.Id)
                .ToList();
            foreach (var receiver in receivers)
            {
                var notification = new NotificationEntity
                {
                    Type = action,
                    Workspace = comment.Task.Workspace,
                    TaskComment = comment,
                    PerformedUser = performedUser,
                    ReceiverUser = receiver,
                    Comment = comment.Comment.Truncate(200)
                };
                await _sessionProvider.CurrentSession.SaveAsync(notification);
                await _gcmNotificationService.SendTaskCommentNotification(
                    comment,
                    receiver,
                    performedUser,
                    action == NotificationActionType.EditEntity
                );    
            }
        }
    }
}
