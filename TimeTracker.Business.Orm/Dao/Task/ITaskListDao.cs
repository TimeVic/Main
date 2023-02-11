using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao.Task;

public interface ITaskListDao: IDomainService
{
    Task<TaskListEntity> CreateTaskListAsync(ProjectEntity project, string name);

    Task<TaskListEntity?> GetById(long taskListId);
    Task<ListDto<TaskListEntity>> GetList(WorkspaceEntity workspace);
}
