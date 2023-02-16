using NHibernate.Criterion;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Dto.Task;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Task;

public class TaskDao: ITaskDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TaskDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
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
        DateTime? notificationTime = null,
        bool isDone = false,
        bool isArchived = false
    )
    {
        var task = new TaskEntity()
        {
            TaskList = taskList,
            User = user,
            Title = title,
            Description = description,
            NotificationTime = notificationTime,
            IsDone = isDone,
            IsArchived = isArchived,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
        };
        await _sessionProvider.CurrentSession.SaveAsync(task);
        return task;
    }
    
    public async Task<ListDto<TaskEntity>> GetList(TaskListEntity taskList, GetTasksFilterDto? filter = null)
    {
        var isArchived = filter?.IsArchived ?? false;
        
        TaskListEntity taskListAlias = null;
        ProjectEntity projectAlias = null;
        UserEntity userAlias = null;
        var query = _sessionProvider.CurrentSession.QueryOver<TaskEntity>()
            .Inner.JoinAlias(item => item.TaskList, () => taskListAlias)
            .Inner.JoinAlias(item => taskListAlias.Project, () => projectAlias)
            .Inner.JoinAlias(item => item.User, () => userAlias)
            .Where(() => taskListAlias.Id == taskList.Id)
            .Where(item => item.IsArchived == isArchived);

        if (filter != null)
        {
            if (filter.AssignedUserId.HasValue)
            {
                query = query.Where(() => userAlias.Id == filter.AssignedUserId);
            }
            if (filter.IsDone.HasValue)
            {
                query = query.Where(item => item.IsDone == filter.IsDone);
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
            .OrderBy(item => item.IsDone).Asc
            .OrderBy(item => item.IsArchived).Asc
            .ThenBy(item => item.UpdateTime).Desc
            .ListAsync<TaskEntity>();
        return new ListDto<TaskEntity>(
            items,
            await query.RowCountAsync()
        );
    }
}
