using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Task;

public interface ITaskListDao: IDomainService
{
    Task<TaskListEntity> CreateTaskList(ProjectEntity project, string name);

    Task<TaskListEntity?> GetById(long taskListId);
}
