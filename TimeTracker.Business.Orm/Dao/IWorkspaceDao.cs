using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IWorkspaceDao: IDomainService
{
    Task<WorkspaceEntity> CreateWorkspace(UserEntity user, string name);

    Task<bool> HasActiveTimeEntriesAsync(WorkspaceEntity workspace);
}
