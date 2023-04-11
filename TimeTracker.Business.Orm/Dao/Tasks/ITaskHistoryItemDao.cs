using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskHistoryItemDao: IDomainService
{
    Task<TaskHistoryItemEntity> Create(TaskEntity task, UserEntity user, bool isNewTask = false);

    Task<ICollection<TaskHistoryItemEntity>> GetBatchToNotify(int timeoutInSeconds = 30);
}
