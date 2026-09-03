using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public class TaskSubTaskDao : ITaskSubTaskDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TaskSubTaskDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<TaskSubTaskEntity?> GetById(Guid subTaskId)
    {
        return await _sessionProvider.CurrentSession.Query<TaskSubTaskEntity>()
            .Fetch(item => item.Task)
            .ThenFetch(task => task.TaskList)
            .ThenFetch(taskList => taskList.Project)
            .ThenFetch(project => project.Client)
            .ThenFetch(client => client.Workspace)
            .Where(item => item.Id == subTaskId)
            .FirstOrDefaultAsync();
    }

    public async Task<TaskSubTaskEntity> AddAsync(TaskEntity task, string title)
    {
        var maxPosition = await _sessionProvider.CurrentSession.Query<TaskSubTaskEntity>()
            .Where(x => x.Task.Id == task.Id)
            .Select(x => (int?)x.PositionIndex)
            .MaxAsync() ?? -1;

        if (task.SubTasks.Any())
        {
            var inMemoryMax = task.SubTasks.Max(x => x.PositionIndex);
            if (inMemoryMax > maxPosition)
            {
                maxPosition = inMemoryMax;
            }
        }

        var subTask = new TaskSubTaskEntity
        {
            Task = task,
            Title = title.Trim(),
            IsCompleted = false,
            PositionIndex = maxPosition + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _sessionProvider.CurrentSession.SaveAsync(subTask);
        task.SubTasks.Add(subTask);
        return subTask;
    }

    public Task<TaskSubTaskEntity> UpdateAsync(TaskSubTaskEntity subTask, string title, bool isCompleted)
    {
        subTask.Title = title.Trim();
        subTask.IsCompleted = isCompleted;
        subTask.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(subTask);
    }

    public async Task DeleteAsync(TaskSubTaskEntity subTask)
    {
        subTask.Task?.SubTasks.Remove(subTask);
        await _sessionProvider.CurrentSession.DeleteAsync(subTask);
    }

    public async Task UpdatePositionsAsync(TaskEntity task, IDictionary<Guid, int> positions)
    {
        var subTasks = await _sessionProvider.CurrentSession.Query<TaskSubTaskEntity>()
            .Where(x => x.Task.Id == task.Id)
            .ToListAsync();

        foreach (var subTask in subTasks)
        {
            if (positions.TryGetValue(subTask.Id, out var newPosition))
            {
                subTask.PositionIndex = newPosition;
                subTask.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
