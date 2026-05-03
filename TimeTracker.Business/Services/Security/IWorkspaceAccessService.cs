using Domain.Abstractions;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Services.Security.Model;

namespace TimeTracker.Business.Services.Security;

public interface IWorkspaceAccessService: IDomainService
{
    public Task<WorkspaceMemberEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        MembershipAccessType access,
        ICollection<ProjectAccessModel>? projectsAccess = null
    );

    Task<bool> RemoveAccessAsync(Guid memberId);
    
    Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user,
        WorkspaceEntity entryWorkspace,
        ProjectEntity? project = null
    );

    Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user,
        ProjectEntity project
    );

    WorkspaceMemberEntity? GetMemberAsync(
        UserEntity user,
        WorkspaceEntity workspace
    );
}
