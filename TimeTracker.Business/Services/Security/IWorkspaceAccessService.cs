using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Services.Security.Model;

namespace TimeTracker.Business.Services.Security;

public interface IWorkspaceAccessService: IDomainService
{
    public Task<WorkspaceMembershipEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        MembershipAccessType access,
        ICollection<ProjectAccessModel>? projectsAccess = null
    );

    Task<bool> RemoveAccessAsync(long membershipId);
    
    Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user,
        WorkspaceEntity entryWorkspace,
        ProjectEntity? project = null
    );

    Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user,
        ProjectEntity project
    );

    Task<WorkspaceMembershipEntity?> GetMembershipAsync(
        UserEntity user,
        WorkspaceEntity workspace
    );
}
