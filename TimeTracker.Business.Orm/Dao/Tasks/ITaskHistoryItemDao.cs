using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskHistoryItemDao: IDomainService
{
    Task Create(TaskEntity task, UserEntity user);
}
