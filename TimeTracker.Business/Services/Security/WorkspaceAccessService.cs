using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Security.Model;

namespace TimeTracker.Business.Services.Security;

public class WorkspaceAccessService: IWorkspaceAccessService
{
    private readonly IDbSessionProvider _sessionProvider;

    public WorkspaceAccessService(
        IDbSessionProvider sessionProvider
    )
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<WorkspaceMembershipEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        MembershipAccessType access,
        ICollection<ProjectAccessModel>? projectsAccess = null
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

        projectsAccess ??= new List<ProjectAccessModel>();
        membership.ProjectAccesses.Clear();
        if (projectsAccess.Any())
        {
            foreach (var projectAccess in projectsAccess.DistinctBy(item => item.Project.Id))
            {
                var accessEntity = new WorkspaceMembershipProjectAccessEntity()
                {
                    Project = projectAccess.Project,
                    HourlyRate = projectAccess.HourlyRate,
                    CreateTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow,
                    WorkspaceMembership = membership
                };
                membership.ProjectAccesses.Add(accessEntity);
            }
        }
        await _sessionProvider.CurrentSession.SaveAsync(membership);
        return membership;
    }

    public async Task<bool> RemoveAccessAsync(long membershipId)
    {
        await _sessionProvider.CurrentSession.Query<WorkspaceMembershipProjectAccessEntity>()
            .Where(item => item.WorkspaceMembership.Id == membershipId)
            .DeleteAsync();
        var counter = await _sessionProvider.CurrentSession.Query<WorkspaceMembershipEntity>()
            .Where(item => item.Id == membershipId)
            .DeleteAsync();
        return counter > 0;
    }
    
    public async Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user, 
        WorkspaceEntity workspace,
        ProjectEntity? project = null
    )
    {
        var member = await GetMembershipAsync(user, workspace);
        return member?.Access;
    }
    
    public async Task<MembershipAccessType?> GetAccessTypeAsync(UserEntity user, ProjectEntity project)
    {
        var member = await GetMembershipAsync(user, project.Workspace);
        if (member == null)
        {
            return null;
        }
        if (member.Access == MembershipAccessType.Manager || member.Access == MembershipAccessType.Owner)
        {
            return member.Access;
        }

        var hasUserAccess = await member.ProjectAccesses.AsQueryable().AnyAsync(
            item => item.Project.Id == project.Id
        );
        if (hasUserAccess)
        {
            return MembershipAccessType.User;
        }
        return null;
    }
    
    public Task<WorkspaceMembershipEntity?> GetMembershipAsync(
        UserEntity user, 
        WorkspaceEntity workspace
    )
    {
        return Task.FromResult(
            workspace.Memberships.FirstOrDefault(item => item.User.Id == user.Id)   
        );
    }
}
