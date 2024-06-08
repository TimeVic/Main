using Domain.Abstractions;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Notification.Center.Handlers;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Business.Services.Notification.Center;

public class NotificationCenterService: INotificationCenterService
{
    private readonly INotificationDao _notificationDao;
    private readonly INotificationCenterHandler<TaskEntity> _taskHandler;
    private readonly INotificationCenterHandler<TaskCommentEntity> _taskCommentHandler;
    private readonly ISecurityManager _securityManager;
    private readonly IDbSessionProvider _sessionProvider;

    public NotificationCenterService(
        INotificationDao notificationDao,
        INotificationCenterHandler<TaskEntity> taskHandler,
        INotificationCenterHandler<TaskCommentEntity> taskCommentHandler,
        ISecurityManager securityManager,
        IDbSessionProvider sessionProvider
    )
    {
        _notificationDao = notificationDao;
        _taskHandler = taskHandler;
        _taskCommentHandler = taskCommentHandler;
        _securityManager = securityManager;
        _sessionProvider = sessionProvider;
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
        if (entity is TaskCommentEntity)
        {
            await _taskCommentHandler.Handle(action, user, entity);
        }
    }

    public async Task<int> GetUnreadCount(UserEntity user, WorkspaceEntity workspace)
    {
        await _securityManager.CheckAccess(AccessLevel.Read, user, workspace);
        return await _notificationDao.GetCount(user, workspace, true);
    }
    
    public async Task<int> MarkAllAsRead(UserEntity user, WorkspaceEntity workspace)
    {
        await _securityManager.CheckAccess(AccessLevel.Read, user, workspace);
        return await _notificationDao.MarkAllAsRead(user, workspace);
    }
    
    public async Task MarkAsRead(NotificationEntity notification)
    {
        if (notification.IsRead)
        {
            return;
        }
        notification.IsRead = true;
        notification.UpdateTime = DateTime.UtcNow;
        await _sessionProvider.CurrentSession.SaveAsync(notification);
    }
    
    public async Task<ListDto<NotificationEntity>> GetList(UserEntity user, WorkspaceEntity workspace, int page)
    {
        await _securityManager.CheckAccess(AccessLevel.Read, user, workspace);
        return await _notificationDao.GetList(user, workspace, page);
    }  
}
