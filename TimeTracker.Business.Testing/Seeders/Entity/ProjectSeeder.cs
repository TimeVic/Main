using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public class ProjectSeeder: IProjectSeeder
{
    private readonly IDataFactory<ProjectEntity> _projectFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IProjectDao _projectDao;

    public ProjectSeeder(
        IDataFactory<ProjectEntity> projectFactory,
        IUserSeeder userSeeder,
        IProjectDao projectDao
    )
    {
        _projectFactory = projectFactory;
        _userSeeder = userSeeder;
        _projectDao = projectDao;
    }
    
    public async Task<ICollection<ProjectEntity>> CreateSeveralAsync(UserEntity user, int count = 1)
    {
        var workspace = user.Workspaces.First();
        var result = new List<ProjectEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _projectFactory.Generate();
            var entry = await _projectDao.CreateAsync(workspace, fakeEntry.Name);;
            result.Add(entry);
        }

        return result;
    }

    public async Task<ICollection<ProjectEntity>> CreateSeveralAsync(int count = 1)
    {
        var user = await _userSeeder.CreateActivatedAsync();
        return await CreateSeveralAsync(user);
    }
}
