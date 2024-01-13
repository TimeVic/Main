using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;

public class GoalsTrackerSeeder: IGoalsTrackerSeeder
{
    private readonly IDataFactory<GoalsTrackerEntity> _goalsTrackerFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IGoalsTrackerDao _trackerDao;
    private readonly IUserDao _userDao;

    public GoalsTrackerSeeder(
        IDataFactory<GoalsTrackerEntity> goalsTrackerFactory,
        IUserSeeder userSeeder,
        IGoalsTrackerDao trackerDao,
        IUserDao userDao
    )
    {
        _goalsTrackerFactory = goalsTrackerFactory;
        _userSeeder = userSeeder;
        _trackerDao = trackerDao;
        _userDao = userDao;
    }
    
    public async Task<GoalsTrackerEntity> CreateAsync(UserEntity user, WorkspaceEntity workspace, int count = 1)
    {
        var fakeEntry = _goalsTrackerFactory.Generate();
        return await _trackerDao.CheckAndCreate(
            user,
            workspace,
            new DateTime(fakeEntry.Year, fakeEntry.Month, 1)
        );
    }
    
    public async Task<ICollection<GoalsTrackerEntity>> CreateSeveralAsync(UserEntity user, WorkspaceEntity workspace, int count = 1)
    {
        var result = new List<GoalsTrackerEntity>();
        for (int i = 0; i < count; i++)
        {
            var fakeEntry = _goalsTrackerFactory.Generate();
            var entry = await _trackerDao.CheckAndCreate(
                user,
                workspace,
                new DateTime(fakeEntry.Year, fakeEntry.Month, 1)
            );
            result.Add(entry);
        }

        return result;
    }
    
    public async Task<ICollection<GoalsTrackerEntity>> CreateSeveralAsync(int count = 1)
    {
        var user = await _userSeeder.CreateActivatedAsync();
        var workspace = (await _userDao.GetUsersWorkspaces(user, MembershipAccessType.Owner)).First();
        return await CreateSeveralAsync(user, workspace);
    }
}
