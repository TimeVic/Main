using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public class TaskHistoryItemDao: ITaskHistoryItemDao
{
    private const string SqlQueryGetItemsToNotify = @"
        select 
        	distinct on (task_id)
        	id
        from task_history_items
        where is_notified is false 
            and is_new_task is false
            and create_time >= :startTime
            and create_time <= :endTime
        order by task_id, create_time desc
        limit 50
    ";
    
    private const string SqlQuerySetNotifyToFalse = @"
        update task_history_items 
        set is_notified = true
        where task_id in (:taskIds) and create_time < :endTime
    ";
    
    private readonly IDbSessionProvider _dbSessionProvider;

    public TaskHistoryItemDao(
        IDbSessionProvider dbSessionProvider    
    )
    {
        _dbSessionProvider = dbSessionProvider;
    }

    public async Task<TaskHistoryItemEntity> Create(TaskEntity task, UserEntity user, bool isNewTask = false)
    {
        var historyItem = new TaskHistoryItemEntity()
        {
            Task = task,
            User = user,
            Title = task.Title,
            Description = task.Description,
            Tags = task.TagsString,
            Attachments = task.AttachmentsString,
            NotificationTime = task.NotificationTime,
            IsDone = task.IsDone,
            IsArchived = task.IsArchived,
            ExternalTaskId = task.ExternalTaskId,
            AssigneeUser = task.User,
            TaskList = task.TaskList,
            IsNewTask = isNewTask,
            CreateTime = DateTime.UtcNow
        };
        await _dbSessionProvider.CurrentSession.SaveAsync(historyItem);
        return historyItem;
    }
    
    public async Task<ICollection<TaskHistoryItemEntity>> GetBatchToNotify(int timeoutInSeconds = 30)
    {
        var endTime = DateTime.UtcNow.AddSeconds(-timeoutInSeconds);
        var transaction = _dbSessionProvider.CurrentSession.BeginTransaction();
        var historyItemIds = await _dbSessionProvider.CurrentSession.CreateSQLQuery(SqlQueryGetItemsToNotify)
            .SetParameter("startTime", DateTime.UtcNow.AddDays(-1))
            .SetParameter("endTime", endTime)
            .ListAsync<long>();

        UserEntity assigneeUserAlias = null;
        UserEntity userAlias = null;
        TaskEntity taskAlias = null;
        TaskListEntity taskListAlias = null;
        var historyItemsToNotify = await _dbSessionProvider
            .CurrentSession
            .QueryOver<TaskHistoryItemEntity>()
            .Inner.JoinAlias(item => item.Task, () => taskAlias)
            .Inner.JoinAlias(item => item.User, () => userAlias)
            .Left.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Left.JoinAlias(item => item.AssigneeUser, () => assigneeUserAlias)
            .WhereRestrictionOn(item => item.Id).IsIn(historyItemIds.ToList())
            .ListAsync();
        if (historyItemsToNotify.Any())
        {
            await _dbSessionProvider.CurrentSession
                .CreateSQLQuery(SqlQuerySetNotifyToFalse)
                .SetParameterList(
                    "taskIds",
                    historyItemsToNotify.Select(item => item.Task.Id).ToArray()
                )
                .SetParameter("endTime", endTime)
                .ExecuteUpdateAsync();
        }
        await transaction.CommitAsync();
        return historyItemsToNotify;
    }
}
