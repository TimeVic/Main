using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public interface ITaskListDao: IDomainService
{
    Task<TaskListEntity> CreateTaskListAsync(ProjectEntity project, string name);

    Task<TaskListEntity?> GetById(Guid taskListId);
    
    Task<ListDto<TaskListEntity>> GetList(WorkspaceEntity workspace);

    Task ArchiveTaskListAsync(TaskListEntity taskList);
}
