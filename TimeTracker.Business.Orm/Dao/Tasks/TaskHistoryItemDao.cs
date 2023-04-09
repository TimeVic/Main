using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public class TaskHistoryItemDao: ITaskHistoryItemDao
{
    private readonly IDbSessionProvider _dbSessionProvider;

    public TaskHistoryItemDao(
        IDbSessionProvider dbSessionProvider    
    )
    {
        _dbSessionProvider = dbSessionProvider;
    }

    public async Task Create(TaskEntity task, UserEntity user)
    {
        var historyItem = new TaskHistoryItemEntity()
        {
            Task = task,
            User = user,
            Title = task.Title,
            Description = task.Description,
            Tags = string.Join(";", task.Tags.Select(item => item.Name)),
            Attachments = string.Join(";", task.Attachments.Select(item => item.Url)),
            NotificationTime = task.NotificationTime,
            IsDone = task.IsDone,
            IsArchived = task.IsArchived,
            ExternalTaskId = task.ExternalTaskId,
            AssigneeUser = task.User,
            TaskList = task.TaskList,
            CreateTime = DateTime.UtcNow
        };
        await _dbSessionProvider.CurrentSession.SaveAsync(historyItem);
    }
}
