using NHibernate;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;
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

    public async Task<WorkspaceMemberEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        MembershipAccessType access,
        ICollection<ProjectAccessModel>? projectsAccess = null
    )
    {
        var member = workspace.Members.FirstOrDefault(item => item.User.Id == user.Id);
        if (member == null)
        {
            member = new WorkspaceMemberEntity()
            {
                User = user,
                Workspace = workspace,
                CreatedAt = DateTime.UtcNow
            };
            workspace.Members.Add(member);
        }
        member.UpdatedAt = DateTime.UtcNow;
        member.Access = access;

        projectsAccess ??= new List<ProjectAccessModel>();
        member.ProjectAccesses.Clear();
        if (projectsAccess.Any())
        {
            foreach (var projectAccess in projectsAccess.DistinctBy(item => item.Project.Id))
            {
                var accessEntity = new WorkspaceMemberProjectAccessEntity()
                {
                    Project = projectAccess.Project,
                    HourlyRate = projectAccess.HourlyRate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    WorkspaceMember = member
                };
                member.ProjectAccesses.Add(accessEntity);
            }
        }
        await _sessionProvider.CurrentSession.SaveAsync(member);
        return member;
    }

    public async Task<bool> RemoveAccessAsync(Guid memberId)
    {
        await _sessionProvider.CurrentSession.Query<WorkspaceMemberProjectAccessEntity>()
            .Where(item => item.WorkspaceMember.Id == memberId)
            .DeleteAsync();
        var counter = await _sessionProvider.CurrentSession.Query<WorkspaceMemberEntity>()
            .Where(item => item.Id == memberId)
            .DeleteAsync();
        return counter > 0;
    }
    
    public async Task<MembershipAccessType?> GetAccessTypeAsync(
        UserEntity user, 
        WorkspaceEntity workspace,
        ProjectEntity? project = null
    )
    {
        var member = GetMemberAsync(user, workspace);
        return member?.Access;
    }
    
    public async Task<MembershipAccessType?> GetAccessTypeAsync(UserEntity user, ProjectEntity project)
    {
        var member = GetMemberAsync(user, project.Client.Workspace);
        if (member == null)
        {
            return null;
        }
        if (member.Access is MembershipAccessType.Manager or MembershipAccessType.Owner)
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
    
    public WorkspaceMemberEntity? GetMemberAsync(
        UserEntity user, 
        WorkspaceEntity workspace
    )
    {
        return workspace.Members.FirstOrDefault(item => item.User.Id == user.Id);
    }
}
