using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class ClientSeeder: IClientSeeder
{
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IClientDao _clientDao;

    public ClientSeeder(
        IDataFactory<ProjectEntity> projectFactory,
        IUserSeeder userSeeder,
        IClientDao clientDao
    )
    {
        _projectFactory = projectFactory;
        _userSeeder = userSeeder;
        _clientDao = clientDao;
    }
    
    public async Task<ICollection<ClientEntity>> CreateSeveralAsync(UserEntity user, int count = 1)
    {
        var workspace = user.Workspaces.First();
        return await CreateSeveralAsync(workspace, count);
    }

    public async Task<ICollection<ClientEntity>> CreateSeveralAsync(WorkspaceEntity workspace, int count = 1)
    {
        var result = new List<ClientEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _projectFactory.Generate();
            var entry = await _clientDao.CreateAsync(workspace, fakeEntry.Name);;
            result.Add(entry);
        }

        return result;
    }
    
    public async Task<ICollection<ClientEntity>> CreateSeveralAsync(int count = 1)
    {
        var user = await _userSeeder.CreateActivatedAsync();
        return await CreateSeveralAsync(user);
    }
}
