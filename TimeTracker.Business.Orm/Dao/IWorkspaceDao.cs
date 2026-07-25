using Domain.Abstractions;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public interface IWorkspaceDao: IDomainService
{
    Task<WorkspaceEntity?> GetById(Guid? id);

    Task<IReadOnlyCollection<WorkspaceEntity>> GetDeletedBeforeAsync(DateTime deletedBefore);

    Task<WorkspaceEntity?> GetDeletedByIdAsync(Guid id);
    
    Task<WorkspaceEntity> CreateWorkspaceAsync(UserEntity user, string name, bool isDefault = false);

    Task<bool> HasActiveTimeEntriesAsync(WorkspaceEntity workspace);

    Task<ListDto<WorkspaceMemberEntity>> GetMembersAsync(WorkspaceEntity workspace, int page);

    Task<WorkspaceMemberEntity> GetMemberAsync(Guid id);

    Task<WorkspaceEntity> UpdateWorkspaceAsync(
        WorkspaceEntity workspace,
        string name,
        CurrencyEntity currency,
        string timeZone,
        string? description
    );
}
