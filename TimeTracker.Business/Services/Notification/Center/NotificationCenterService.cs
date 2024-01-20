using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Center.Handlers;

namespace TimeTracker.Business.Services.Notification.Center;

public class NotificationCenterService: INotificationCenterService
{
    private readonly INotificationDao _notificationDao;
    private readonly INotificationCenterHandler<TaskEntity> _taskHandler;

    public NotificationCenterService(
        INotificationDao notificationDao,
        INotificationCenterHandler<TaskEntity> taskHandler
    )
    {
        _notificationDao = notificationDao;
        _taskHandler = taskHandler;
    }

    public async Task Push<T>(
        NotificationActionType action,
        UserEntity user,
        T entity
    ) where T: IEntity
    {
        if (entity is TaskEntity)
        {
            await _taskHandler.Handle(action, user, entity);
        }
    }

    public Task<int> GetUnreadCount(UserEntity user, WorkspaceEntity workspace)
    {
        return _notificationDao.GetCount(user, workspace, true);
    }
    
    public Task<int> MarkAllAsRead(UserEntity user, WorkspaceEntity workspace)
    {
        return _notificationDao.MarkAllAsRead(user, workspace);
    }
    
    public Task<ListDto<NotificationEntity>> GetList(UserEntity user, WorkspaceEntity workspace, int page)
    {
        return _notificationDao.GetList(user, workspace, page);
    }  
}
