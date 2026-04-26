using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao.System;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public class TaskDao: ITaskDao
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly ITaskHistoryItemDao _taskHistoryItemDao;
    private readonly ISequenceDao _sequenceDao;

    public TaskDao(
        IDbSessionProvider sessionProvider,
        ITaskHistoryItemDao taskHistoryItemDao,
        ISequenceDao sequenceDao
    )
    {
        _sessionProvider = sessionProvider;
        _taskHistoryItemDao = taskHistoryItemDao;
        _sequenceDao = sequenceDao;
    }

    public async Task<TaskEntity?> GetById(Guid taskId)
    {
        return await _sessionProvider.CurrentSession.Query<TaskEntity>()
            .Where(item => item.Id == taskId)
            .FirstOrDefaultAsync();
    }
    
    public async Task UpdatePositions(WorkspaceEntity workspace, IDictionary<Guid, int> items)
    {
        var listDto = await GetList(workspace);
        foreach (var task in listDto.Items)
        {
            var indexToUpdate = items.Where(y => y.Key == task.Id)
                .Select(x => (int?)x.Value)
                .FirstOrDefault();
            if (indexToUpdate == null)
                continue;
            task.PositionIndex = indexToUpdate.Value;
            await _sessionProvider.CurrentSession.UpdateAsync(task);    
        }
    }
    
    public async Task<TaskEntity?> GetByWorkspaceTaskId(
        Guid workspaceId,
        long workspaceTaskId
    )
    {
        TaskListEntity taskListAlias = null!;
        ProjectEntity projectAlias = null!;
        WorkspaceEntity workspaceAlias = null!;
        return await _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias!.Project, () => projectAlias)
            .Inner.JoinAlias(item => projectAlias!.Workspace, () => workspaceAlias)
            .Where(
                item => item.TaskId == workspaceTaskId
                && workspaceAlias!.Id == workspaceId
            )
            .SingleOrDefaultAsync();
    }
    
    public async Task<TaskEntity> AddTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        TimeSpan? originalEstimate = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        TaskStatus status = TaskStatus.Backlog,
        TaskPriority priority = TaskPriority.Low,
        bool isArchived = false,
        ExternalSourceType externalSourceType = ExternalSourceType.Manual
    )
    {
        var taskId = await _sequenceDao.GetNextValue(taskList.Project.Workspace);
        var nextPositionIndex = await GetNextPositionIndexAsync(taskList.Id);
        var task = new TaskEntity()
        {
            TaskId = taskId,
            PositionIndex = nextPositionIndex,
            TaskList = taskList,
            User = user,
            Title = title,
            Description = description,
            OriginalEstimate = originalEstimate,
            ExternalSourceType = externalSourceType,
            Status = status,
            Priority = priority,
            IsArchived = isArchived,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        SetStartEndTime(task, startTime, endTime);

        await _sessionProvider.CurrentSession.SaveAsync(task);
        await _taskHistoryItemDao.Create(task, user, true);
        return task;
    }
    
    public async Task<TaskEntity> UpdateTaskAsync(
        TaskEntity task,
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        TimeSpan? originalEstimate = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        TaskStatus status = TaskStatus.Backlog,
        TaskPriority priority = TaskPriority.Low,
        bool isArchived = false,
        IEnumerable<TagEntity>? tags = null,
        DateTime? reminderTime = null,
        bool isAddHistoryItem = true
    )
    {
        task.TaskList = taskList;
        task.User = user;
        task.Title = title;
        task.Description = description;
        task.OriginalEstimate = originalEstimate;
        task.Status = status;
        task.Priority = priority;
        task.IsArchived = isArchived;
        task.UpdatedAt = DateTime.UtcNow;
        task.ReminderTime = reminderTime;
        SetStartEndTime(task, startTime, endTime);
        
        task.Tags.Clear();
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                task.Tags.Add(tag);
            }
        }

        await _sessionProvider.CurrentSession.SaveAsync(task);
        if (isAddHistoryItem)
        {
            await _taskHistoryItemDao.Create(task, user, false);
        }
        return task;
    }

    public async Task<ListDto<TaskEntity>> GetList(
        WorkspaceEntity? workspace = null,
        TaskListEntity? taskList = null,
        GetTasksFilterDto? filter = null
    )
    {
        if (workspace == null && taskList == null)
        {
            throw new ArgumentNullException($"{nameof(workspace)}, {nameof(taskList)}");
        }
        
        TaskListEntity taskListAlias = null!;
        ProjectEntity projectAlias = null!;
        WorkspaceEntity workspaceAlias = null!;
        UserEntity userAlias = null!;
        var query = _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias!.Project, () => projectAlias)
            .Inner.JoinAlias(item => projectAlias!.Workspace, () => workspaceAlias)
            .Inner.JoinAlias(item => item.User, () => userAlias);

        if (taskList != null)
        {
            query = query.Where(() => taskListAlias!.Id == taskList.Id);
        }
        else
        {
            query = query.Where(() => workspaceAlias!.Id == workspace!.Id);
        }

        if (filter != null)
        {
            if (filter.IsArchived.HasValue)
            {
                query = query.Where(item => item.IsArchived == filter.IsArchived);
            }
            if (filter.AssignedUserId.HasValue)
            {
                query = query.Where(() => userAlias!.Id == filter.AssignedUserId);
            }
            if (filter.Status.HasValue)
            {
                query = query.Where(item => item.Status == filter.Status);
            }
            else if (filter.Statuses != null && filter.Statuses.Any())
            {
                query = query.WhereRestrictionOn(item => item.Status)
                    .IsIn(filter.Statuses.ToList());
            }
            if (!string.IsNullOrWhiteSpace(filter.SearchString))
            {
                query = query.Where(
                    item => item.Title.IsInsensitiveLike(filter.SearchString.ToLower(), MatchMode.Anywhere)
                    || item.Description.IsInsensitiveLike(filter.SearchString.ToLower(), MatchMode.Anywhere)
                );
            }
            if (filter is {StartTime: not null, EndTime: not null})
            {
                query = query.Where(
                    item => item.StartTime >= filter.StartTime && item.StartTime <= filter.EndTime
                        || item.EndTime >= filter.StartTime && item.EndTime <= filter.EndTime
                );
            }
        }

        if (taskList != null)
        {
            query = query.OrderBy(item => item.IsArchived).Asc
                .ThenBy(item => item.PositionIndex).Asc
                .ThenBy(item => item.CreatedAt).Asc;
        }
        else
        {
            query = query.OrderBy(item => item.Priority).Asc
                .ThenBy(item => item.IsArchived).Asc
                .ThenBy(item => item.UpdatedAt).Desc;
        }

        var items = await query
            .Take(1000)
            .ListAsync<TaskEntity>();
        return new ListDto<TaskEntity>(
            items,
            await query.RowCountAsync()
        );
    }

    private async Task<int> GetNextPositionIndexAsync(Guid taskListId)
    {
        var maxPositionIndex = await _sessionProvider.CurrentSession.Query<TaskEntity>()
            .Where(item => item.TaskList.Id == taskListId)
            .Select(item => (int?)item.PositionIndex)
            .MaxAsync();

        return (maxPositionIndex ?? -1) + 1;
    }
    
    private void SetStartEndTime(TaskEntity task, DateTime? startTime, DateTime? endTime)
    {
        if (endTime < startTime)
        {
            throw new ValidationException("End Time can not be less than Start Time");
        }
        task.StartTime = startTime;
        task.EndTime = endTime;
    }
    
    public async Task<ICollection<TaskEntity>> GetTasksToRemind()
    {
        var timeToRemind = DateTime.UtcNow + GlobalConstants.TaskReminderTimeout;
        TaskListEntity taskListAlias = null!;
        ProjectEntity projectAlias = null!;
        WorkspaceEntity workspaceAlias = null!;
        UserEntity userAlias = null!;
        return await _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias!.Project, () => projectAlias)
            .Inner.JoinAlias(item => projectAlias!.Workspace, () => workspaceAlias)
            .Inner.JoinAlias(item => item.User, () => userAlias)
            .Where(item => item.IsArchived == false)
            .Where(
                item => item.ReminderTime != null 
                    && item.ReminderTime < timeToRemind
                    && (
                        item.RemindedTime == null
                        || item.ReminderTime != item.RemindedTime    
                    )
            )
            .Take(100)
            .ListAsync<TaskEntity>();
    }

    public async Task<IDictionary<Guid, TimeSpan>> GetTrackedDurationByTaskIds(ICollection<Guid> taskIds)
    {
        if (!taskIds.Any())
        {
            return new Dictionary<Guid, TimeSpan>();
        }

        var items = await _sessionProvider.CurrentSession.CreateSQLQuery(@"
                SELECT
                    te.internal_task_id AS TaskId,
                    COALESCE(SUM(EXTRACT(EPOCH FROM (te.end_time - te.start_time))), 0) AS TrackedSeconds
                FROM time_entries te
                WHERE te.internal_task_id IN (:taskIds)
                    AND te.end_time IS NOT NULL
                    AND te.is_marked_to_delete = false
                GROUP BY te.internal_task_id
            ")
            .AddScalar("TaskId", NHibernateUtil.Guid)
            .AddScalar("TrackedSeconds", NHibernateUtil.Double)
            .SetParameterList("taskIds", taskIds)
            .SetResultTransformer(Transformers.AliasToBean<TaskTrackedDurationItemDto>())
            .ListAsync<TaskTrackedDurationItemDto>();

        return items.ToDictionary(
            item => item.TaskId,
            item => TimeSpan.FromSeconds(item.TrackedSeconds)
        );
    }
}
