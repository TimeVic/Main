using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Orm.Dao;

public interface IWorkspaceDao: IDomainService
{
    Task<WorkspaceEntity?> GetByIdAsync(long id);
    
    Task<WorkspaceEntity> CreateWorkspaceAsync(UserEntity user, string name, bool isDefault = false);

    Task<bool> HasActiveTimeEntriesAsync(WorkspaceEntity workspace);

    Task<ListDto<WorkspaceMembershipEntity>> GetMembershipsAsync(WorkspaceEntity workspace, int page);

    Task<WorkspaceMembershipEntity> GetMembershipAsync(long id);

    Task<WorkspaceEntity> UpdateWorkspaceAsync(WorkspaceEntity workspace, string name);
}
