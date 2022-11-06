using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;

namespace TimeTracker.Business.Orm.Dao;

public class ProjectDao: IProjectDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public ProjectDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<ProjectEntity> CreateAsync(WorkspaceEntity workspace, string name)
    {
        var project = new ProjectEntity()
        {
            Name = name,
            Workspace = workspace,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow
        };
        workspace.Projects.Add(project);
        await _sessionProvider.CurrentSession.SaveAsync(project);
        return project;
    }

    public async Task<ProjectEntity?> GetById(long? projectId)
    {
        if (projectId == null)
            return null;

        return await _sessionProvider.CurrentSession.Query<ProjectEntity>()
            .Where(item => item.Id == projectId)
            .FirstOrDefaultAsync();
    }
    
    public async Task<ListDto<ProjectEntity>> GetAvailableForUserListAsync(
        WorkspaceEntity workspace,
        UserEntity? user = null,
        MembershipAccessType? accessType = null
    )
    {
        var query = _sessionProvider.CurrentSession.QueryOver<ProjectEntity>()
            .Select(
                Projections.Group<ProjectEntity>(x => x.Id)
            )
            .Where(item => item.Workspace.Id == workspace.Id);

        if (
            user != null 
            && accessType != MembershipAccessType.Manager 
            && accessType != MembershipAccessType.Owner
        )
        {
            // Is not owner
            WorkspaceMembershipProjectAccessEntity projectAccessAlias = null;
            WorkspaceMembershipEntity workspaceMembershipAlias = null;
            UserEntity userAlias = null;
            query = query.Inner.JoinAlias(item => item.MembershipProjectAccess, () => projectAccessAlias)
                .Inner.JoinAlias(() => projectAccessAlias.WorkspaceMembership, () => workspaceMembershipAlias)
                .Inner.JoinAlias(() => workspaceMembershipAlias.User, () => userAlias)
                .And(item => userAlias.Id == user.Id);
        }

        var projectIds = await query.ListAsync<long>();
        var projects = await _sessionProvider.CurrentSession.Query<ProjectEntity>()
            .Where(item => projectIds.Contains(item.Id))
            .OrderByDescending(item => item.Name)
            .ToListAsync();
        
        return new ListDto<ProjectEntity>(projects, projects.Count);
    }
}
