using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao;

public class ProjectDao: IProjectDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public ProjectDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<ProjectEntity> CreateAsync(ClientEntity client, string name)
    {
        var project = new ProjectEntity()
        {
            Name = name,
            Client = client,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        client.Projects.Add(project);
        await _sessionProvider.CurrentSession.SaveAsync(project);
        return project;
    }

    public async Task<ProjectEntity?> GetById(Guid? projectId, bool isOnlyActive = true)
    {
        if (projectId == null)
            return null;

        return await _sessionProvider.CurrentSession.Query<ProjectEntity>()
            .Where(item => item.Id == projectId)
            .Where(item => !isOnlyActive || !item.IsArchived)
            .FirstOrDefaultAsync();
    }
    
    public async Task ArchiveProject(ProjectEntity project)
    {
        if (project.IsArchived)
        {
            throw new DataValidationException();
        }
        project.IsArchived = true;
        await _sessionProvider.CurrentSession.SaveAsync(project);
        foreach (var taskList in project.TaskLists)
        {
            taskList.IsArchived = true;
            await _sessionProvider.CurrentSession.SaveAsync(project);
        }
    }
    
    public async Task<ListDto<ProjectEntity>> GetAvailableForUserListAsync(
        WorkspaceEntity workspace,
        UserEntity? user = null,
        MembershipAccessType? accessType = null
    )
    {
        var query = _sessionProvider.CurrentSession.Query<ProjectEntity>()
            .Where(item => item.Client != null && item.Client.Workspace.Id == workspace.Id)
            .Where(item => !item.IsArchived);

        if (
            user != null 
            && accessType != MembershipAccessType.Manager 
            && accessType != MembershipAccessType.Owner
        )
        {
            // Is not owner
            query = query.Where(item => item.MemberProjectAccess.Any(
                access => access.WorkspaceMember.User.Id == user.Id
            ));
        }

        var projectIds = await query
            .Select(item => item.Id)
            .Distinct()
            .ToListAsync();
        var projects = await _sessionProvider.CurrentSession.Query<ProjectEntity>()
            .Where(item => projectIds.Contains(item.Id))
            .OrderByDescending(item => item.Name)
            .ToListAsync();
        
        return new ListDto<ProjectEntity>(projects, projects.Count);
    }
}
