using NHibernate.Criterion;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Tasks;
using TimeTracker.Business.Orm.Entities;
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
        if (endTime < startTime)
        {
            throw new ValidationException("End Time can not be less than Start Time");
        }

        var task = new TaskEntity()
        {
            TaskList = taskList,
            User = user,
            Title = title,
            Description = description,
            StartTime = startTime,
            EndTime = endTime,
            Status = status,
            Priority = priority,
            IsArchived = isArchived,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
        };

        await _sessionProvider.CurrentSession.SaveAsync(task);
        await _taskHistoryItemDao.Create(task, user, true);
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
        }

        var items = await query
            .OrderBy(item => item.Priority).Asc
            .OrderBy(item => item.IsArchived).Asc
            .ThenBy(item => item.UpdateTime).Desc
            .ListAsync<TaskEntity>();
        return new ListDto<TaskEntity>(
            items,
            await query.RowCountAsync()
        );
    }
}
