using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class WorkspaceSeeder: IWorkspaceSeeder
{
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly IWorkspaceDao _workspaceDao;

    public WorkspaceSeeder(
        IDataFactory<WorkspaceEntity> workspaceFactory,
        IWorkspaceDao workspaceDao
    )
    {
        _workspaceFactory = workspaceFactory;
        _workspaceDao = workspaceDao;
    }

    public async Task<ICollection<WorkspaceEntity>> CreateSeveralAsync(UserEntity user, int count = 1)
    {
        var result = new List<WorkspaceEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _workspaceFactory.Generate();
            var entry = await _workspaceDao.CreateWorkspace(user, fakeEntry.Name);
            result.Add(entry);
        }

        return result;
    }
}
