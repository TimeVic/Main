using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class ProjectSeeder: IProjectSeeder
{
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectDao _projectDao;
    private readonly IClientSeeder _clientSeeder;

    public ProjectSeeder(
        IDataFactory<ProjectEntity> projectFactory,
        IUserSeeder userSeeder,
        IProjectDao projectDao,
        IClientSeeder clientSeeder
    )
    {
        _projectFactory = projectFactory;
        _userSeeder = userSeeder;
        _projectDao = projectDao;
        _clientSeeder = clientSeeder;
    }

    public async Task<ICollection<ProjectEntity>> CreateSeveralAsync(WorkspaceEntity workspace, int count = 1)
    {
        var result = new List<ProjectEntity>();
        var client = (await _clientSeeder.CreateSeveralAsync(workspace)).First();
        for (int i = 0; i < count; i++)
        {
            result.Add(
                await CreateAsync(workspace, client)
            );
        }

        return result;
    }
    
    public async Task<ProjectEntity> CreateAsync(WorkspaceEntity workspace, ClientEntity? client = null)
    {
        client ??= (await _clientSeeder.CreateSeveralAsync(workspace)).First();
        var fakeEntry = _projectFactory.Generate();
        var entry = await _projectDao.CreateAsync(workspace, fakeEntry.Name);
        entry.SetClient(client);

        return entry;
    }
}
