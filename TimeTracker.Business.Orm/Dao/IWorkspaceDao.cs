using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public interface IWorkspaceDao: IDomainService
{
    Task<WorkspaceEntity> CreateWorkspaceAsync(UserEntity user, string name, bool isDefault = false);

    Task<bool> HasActiveTimeEntriesAsync(WorkspaceEntity workspace);
}
