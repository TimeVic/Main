using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskHistoryItemDao: IDomainService
{
    Task<TaskHistoryItemEntity> Create(TaskEntity task, UserEntity user, bool isNewTask = false);

    Task<ICollection<TaskHistoryItemEntity>> GetBatchToNotify(int timeoutInSeconds = 30);

    Task<TaskHistoryItemEntity?> GetByIdAsync(Guid id);
}
