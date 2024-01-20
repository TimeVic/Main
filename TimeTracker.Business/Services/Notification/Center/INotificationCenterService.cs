using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Services.Notification.Center;

public interface INotificationCenterService: IDomainService
{
    Task Push<T>(
        NotificationActionType action,
        UserEntity user,
        T entity
    ) where T : IEntity;

    Task<int> GetUnreadCount(UserEntity user, WorkspaceEntity workspace);

    Task<int> MarkAllAsRead(UserEntity user, WorkspaceEntity workspace);

    Task<ListDto<NotificationEntity>> GetList(UserEntity user, WorkspaceEntity workspace, int page = 1);
}
