using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

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
        await _sessionProvider.CurrentSession.SaveAsync(workspace);
        return project;
    }
    
    public async Task<ICollection<ProjectEntity>> GetByUser(UserEntity user)
    {
        WorkspaceEntity workspaceAlias = null;
        var query = _sessionProvider.CurrentSession.QueryOver<ProjectEntity>()
            .Inner.JoinAlias(item => item.Workspace, () => workspaceAlias)
            .And(() => workspaceAlias.User.Id == user.Id);
        return await query.ListAsync();
    }
    
    public async Task<ListDto<ProjectEntity>> GetListAsync(WorkspaceEntity workspace)
    {
        var query = _sessionProvider.CurrentSession.Query<ProjectEntity>()
            .Where(item => item.Workspace.Id == workspace.Id);
        
        var items = await query
            .OrderByDescending(item => item.Name)
            .ToListAsync();
        return new ListDto<ProjectEntity>(
            items,
            await query.CountAsync()
        );
    }
    
    public async Task<bool> HasAccessAsync(UserEntity user, ProjectEntity? entity)
    {
        if (entity == null)
        {
            return false;
        }

        ProjectEntity projectAlias = null;
        var itemsWithAccessCount = await _sessionProvider.CurrentSession.QueryOver<WorkspaceEntity>()
            .Inner.JoinAlias(item => item.Projects, () => projectAlias)
            .Where(item => item.User.Id == user.Id)
            .And(() => projectAlias.Id == entity.Id)
            .RowCountAsync();
        return itemsWithAccessCount > 0;
    }
}
