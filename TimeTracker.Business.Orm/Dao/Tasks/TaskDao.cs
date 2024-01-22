using NHibernate.Criterion;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
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

    public TaskDao(
        IDbSessionProvider sessionProvider,
        ITaskHistoryItemDao taskHistoryItemDao
    )
    {
        _sessionProvider = sessionProvider;
        _taskHistoryItemDao = taskHistoryItemDao;
    }

    public async Task<TaskEntity?> GetById(long taskListId)
    {
        return await _sessionProvider.CurrentSession.GetAsync<TaskEntity>(taskListId);
    }
    
    public async Task UpdatePositions(WorkspaceEntity workspace, IDictionary<long, int> items)
    {
        var listDto = await GetList(workspace);
        foreach (var task in listDto.Items)
        {
            var indexToUpdate = items.Where(y => y.Key == task.TaskId)
                .Select(x => (int?)x.Value)
                .FirstOrDefault();
            if (indexToUpdate == null)
                continue;
            task.PositionIndex = indexToUpdate.Value;
            await _sessionProvider.CurrentSession.UpdateAsync(task);    
        }
    }
    
    public async Task<TaskEntity?> GetByWorkspaceTaskId(
        long workspaceId,
        long workspaceTaskId
    )
    {
        TaskListEntity taskListAlias = null;
        ProjectEntity projectAlias = null;
        WorkspaceEntity workspaceAlias = null;
        return await _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias.Project, () => projectAlias)
            .Inner.JoinAlias(item => projectAlias.Workspace, () => workspaceAlias)
            .Where(
                item => item.TaskId == workspaceTaskId
                && workspaceAlias.Id == workspaceId
            )
            .SingleOrDefaultAsync();
    }
    
    public async Task<TaskEntity> AddTaskAsync(
        TaskListEntity taskList,
        UserEntity user,
        string title,
        string? description = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        TaskStatus status = TaskStatus.Backlog,
        TaskPriority priority = TaskPriority.Low,
        bool isArchived = false
    )
    {
        var task = new TaskEntity()
        {
            TaskId = await GetNextTaskId(taskList.Project),
            TaskList = taskList,
            User = user,
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            IsArchived = isArchived,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
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
        task.Status = status;
        task.Priority = priority;
        task.IsArchived = isArchived;
        task.UpdateTime = DateTime.UtcNow;
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

        var isArchived = filter?.IsArchived ?? false;
        
        TaskListEntity taskListAlias = null;
        ProjectEntity projectAlias = null;
        WorkspaceEntity workspaceAlias = null;
        UserEntity userAlias = null;
        var query = _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias.Project, () => projectAlias)
            .Inner.JoinAlias(item => projectAlias.Workspace, () => workspaceAlias)
            .Inner.JoinAlias(item => item.User, () => userAlias)
            .Where(item => item.IsArchived == isArchived);

        if (taskList != null)
        {
            query = query.Where(() => taskListAlias.Id == taskList.Id);
        }
        else
        {
            query = query.Where(() => workspaceAlias.Id == workspace.Id);
        }

        if (filter != null)
        {
            if (filter.AssignedUserId.HasValue)
            {
                query = query.Where(() => userAlias.Id == filter.AssignedUserId);
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
                    item => item.Title.IsLike(filter.SearchString.ToLower(), MatchMode.Anywhere)
                    || item.Description.IsLike(filter.SearchString.ToLower(), MatchMode.Anywhere)
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

        var items = await query
            .OrderBy(item => item.Priority).Asc
            .OrderBy(item => item.IsArchived).Asc
            .ThenBy(item => item.UpdateTime).Desc
            .Take(1000)
            .ListAsync<TaskEntity>();
        return new ListDto<TaskEntity>(
            items,
            await query.RowCountAsync()
        );
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
    
    private async Task<long> GetNextTaskId(ProjectEntity? project)
    {
        TaskListEntity taskListAlias = null;
        ProjectEntity projectAlias = null;
        WorkspaceEntity workspaceAlias = null;
        UserEntity userAlias = null;
        var existsTaskWithMaxId = (await _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias.Project, () => projectAlias)
            .Inner.JoinAlias(item => projectAlias.Workspace, () => workspaceAlias)
            .Inner.JoinAlias(item => item.User, () => userAlias)
            .Where(item => workspaceAlias.Id == project.Workspace.Id)
            .ThenBy(item => item.TaskId).Desc
            .Take(1)
            .ListAsync()).FirstOrDefault();
        if (existsTaskWithMaxId == null)
        {
            return 1;
        }
        return existsTaskWithMaxId.TaskId + 1;
    }
    
    public async Task<ICollection<TaskEntity>> GetTasksToRemind()
    {
        var timeToRemind = DateTime.UtcNow + GlobalConstants.TaskReminderTimeout;
        TaskListEntity taskListAlias = null;
        ProjectEntity projectAlias = null;
        WorkspaceEntity workspaceAlias = null;
        UserEntity userAlias = null;
        return await _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias.Project, () => projectAlias)
            .Inner.JoinAlias(item => projectAlias.Workspace, () => workspaceAlias)
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
}
