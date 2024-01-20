using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public class NotificationDao : INotificationDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public NotificationDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<int> GetCount(UserEntity user, WorkspaceEntity workspace, bool isUnread = false)
    {
        var query = _sessionProvider.CurrentSession.Query<NotificationEntity>()
            .Where(item => item.ReceiverUser == user)
            .Where(item => item.Workspace == workspace);
        if (isUnread)
            query = query.Where(item => item.IsRead == false);
        return await query.CountAsync();
    }

    public async Task<int> MarkAllAsRead(UserEntity user, WorkspaceEntity workspace)
    {
        return await _sessionProvider.CurrentSession.Query<NotificationEntity>()
            .Where(item => item.ReceiverUser == user)
            .Where(item => item.Workspace == workspace)
            .UpdateAsync(item => new
            {
                IsRead = true,
                UpdateTime = DateTime.UtcNow
            });
    }
}
