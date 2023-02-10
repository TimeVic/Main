using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Task;

public interface ITaskListDao: IDomainService
{
    Task<TaskListEntity> CreateList(ProjectEntity project, string name);
}
