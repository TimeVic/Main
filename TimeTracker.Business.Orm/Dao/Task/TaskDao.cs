using Persistence.Transactions.Behaviors;
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
}
