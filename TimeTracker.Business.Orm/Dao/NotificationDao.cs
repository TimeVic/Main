using NHibernate.Criterion;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Notifications;
using TimeTracker.Business.Orm.Entities.Tasks;
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
                UpdatedAt = DateTime.UtcNow
            });
    }
    
    public async Task<ListDto<NotificationEntity>> GetList(
        UserEntity user,
        WorkspaceEntity workspace,
        int page
    )
    {        
        TaskEntity taskAlias = null;
        TaskListEntity taskListAlias = null;
        UserEntity taskUserAlias = null;
        TaskCommentEntity taskCommentAlias = null;
        WorkspaceEntity workspaceAlias = null;
        UserEntity performedUserAlias = null;
        UserEntity receivedUserAlias = null;
        UserEntity userAlias = null;
        
        var query = _sessionProvider.CurrentSession.QueryOver<NotificationEntity>()
            .Left.JoinAlias(item => item.Task, () => taskAlias)
            .Left.JoinAlias(item => taskAlias!.TaskList, () => taskListAlias)
            .Left.JoinAlias(item => taskAlias!.User, () => taskUserAlias)
            .Left.JoinAlias(item => item.TaskComment, () => taskCommentAlias)
            .Inner.JoinAlias(item => item.PerformedUser, () => performedUserAlias)
            .Inner.JoinAlias(item => item.ReceiverUser, () => receivedUserAlias)
            .Inner.JoinAlias(item => item.Workspace, () => workspaceAlias)
            .Where(item => item.ReceiverUser == user)
            .Where(item => workspaceAlias!.Id == workspace.Id);

        var offset = PaginationUtils.CalculateOffset(page);
        var items = await query
            .OrderBy(item => item.CreatedAt).Desc()
            .Skip(offset)
            .Take(GlobalConstants.ListPageSize)
            .ListAsync();
        return new ListDto<NotificationEntity>(
            items,
            await query.RowCountAsync()
        );
    }
}
