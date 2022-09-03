using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Orm.Dao;

public class ProjectDao: IProjectDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public ProjectDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<ProjectEntity> Create(WorkspaceEntity workspace, string name)
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
}
