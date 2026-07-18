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
        TaskEntity taskAlias = null!;
        TaskListEntity taskListAlias = null!;
        UserEntity taskUserAlias = null!;
        TaskCommentEntity taskCommentAlias = null!;
        WorkspaceEntity workspaceAlias = null!;
        UserEntity performedUserAlias = null!;
        UserEntity receivedUserAlias = null!;
        
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
        var totalCount = await query.RowCountAsync();
        var notificationIds = await query
            .Select(item => item.Id)
            .OrderBy(item => item.CreatedAt).Desc()
            .Skip(offset)
            .Take(GlobalConstants.ListPageSize)
            .ListAsync<Guid>();
        if (!notificationIds.Any())
        {
            return new ListDto<NotificationEntity>(new List<NotificationEntity>(), totalCount);
        }

        var notifications = await _sessionProvider.CurrentSession.Query<NotificationEntity>()
            .Where(item => notificationIds.Contains(item.Id))
            .Fetch(item => item.PerformedUser)
            .ThenFetch(user => user.Language)
            .Fetch(item => item.ReceiverUser)
            .ThenFetch(user => user.Language)
            .Fetch(item => item.Task)
            .ThenFetch(task => task!.TaskList)
            .ThenFetch(taskList => taskList.Project)
            .ThenFetch(project => project.Client)
            .ThenFetch(client => client.Workspace)
            .Fetch(item => item.Task)
            .ThenFetch(task => task!.User)
            .ThenFetch(user => user.Language)
            .Fetch(item => item.TaskComment)
            .ThenFetch(comment => comment!.User)
            .ThenFetch(user => user!.Language)
            .ToListAsync();

        var taskIds = notifications
            .Select(item => item.Task?.Id)
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .Distinct()
            .ToList();
        if (taskIds.Any())
        {
            await _sessionProvider.CurrentSession.Query<TaskEntity>()
                .Where(item => taskIds.Contains(item.Id))
                .FetchMany(item => item.Tags)
                .ToListAsync();
        }

        var taskCommentIds = notifications
            .Where(item => item.TaskComment != null)
            .Select(item => item.TaskComment!.Id)
            .Distinct()
            .ToList();
        if (taskCommentIds.Any())
        {
            await _sessionProvider.CurrentSession.Query<TaskCommentEntity>()
                .Where(item => taskCommentIds.Contains(item.Id))
                .FetchMany(item => item.Attachments)
                .ToListAsync();
            await _sessionProvider.CurrentSession.Query<TaskCommentEntity>()
                .Where(item => taskCommentIds.Contains(item.Id))
                .FetchMany(item => item.Watchers)
                .ToListAsync();
        }

        var userIds = notifications
            .SelectMany(item => new[]
            {
                item.PerformedUser.Id,
                item.ReceiverUser.Id,
                item.Task?.User.Id,
                item.TaskComment?.User?.Id
            }
            .Concat(item.TaskComment?.Watchers.Select(watcher => (Guid?)watcher.Id) ?? []))
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .Distinct()
            .ToList();
        await _sessionProvider.CurrentSession.Query<UserEntity>()
            .Where(item => userIds.Contains(item.Id))
            .FetchMany(item => item.Avatars)
            .ToListAsync();

        var notificationsById = notifications.ToDictionary(item => item.Id);
        var items = notificationIds
            .Select(notificationId => notificationsById[notificationId])
            .ToList();

        return new ListDto<NotificationEntity>(
            items,
            totalCount
        );
    }
}
