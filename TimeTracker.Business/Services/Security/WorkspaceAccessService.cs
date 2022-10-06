using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Services.Security;

public class WorkspaceAccessService: IWorkspaceAccessService
{
    private readonly IDbSessionProvider _sessionProvider;

    public WorkspaceAccessService(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<WorkspaceMembershipEntity> ShareAccess(
        WorkspaceEntity workspace,
        UserEntity user,
        MembershipAccessType access,
        ICollection<ProjectEntity>? projects = null
    )
    {
        var membership = workspace.Memberships.FirstOrDefault(item => item.User.Id == user.Id);
        if (membership == null)
        {
            membership = new WorkspaceMembershipEntity()
            {
                User = user,
                Workspace = workspace,
                CreateTime = DateTime.UtcNow
            };
            workspace.Memberships.Add(membership);
        }
        membership.UpdateTime = DateTime.UtcNow;
        membership.Access = access;

        projects ??= new List<ProjectEntity>();
        membership.ProjectAccesses.Clear();
        if (projects.Any() && membership.Access != MembershipAccessType.Manager)
        {
            foreach (var project in projects)
            {
                var projectAccess = new WorkspaceMembershipProjectAccessEntity()
                {
                    Project = project,
                    CreateTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow,
                    WorkspaceMembership = membership
                };
                membership.ProjectAccesses.Add(projectAccess);
            }
        }
        await _sessionProvider.CurrentSession.SaveAsync(membership);
        return membership;
    }
}
