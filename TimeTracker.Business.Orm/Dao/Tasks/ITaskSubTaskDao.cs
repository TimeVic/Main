using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.Tasks;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskSubTaskDao : IDomainService
{
    Task<TaskSubTaskEntity?> GetById(Guid subTaskId);

    Task<TaskSubTaskEntity> AddAsync(TaskEntity task, string title);

    Task<TaskSubTaskEntity> UpdateAsync(TaskSubTaskEntity subTask, string title, bool isCompleted);

    Task DeleteAsync(TaskSubTaskEntity subTask);

    Task UpdatePositionsAsync(TaskEntity task, IDictionary<Guid, int> positions);
}
