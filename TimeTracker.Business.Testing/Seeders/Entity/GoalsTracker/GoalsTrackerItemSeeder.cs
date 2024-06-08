using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.GoalsTracker;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;

namespace TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;

public class GoalsTrackerItemsSeeder: IGoalsTrackerItemsSeeder
{
    private readonly IDataFactory<GoalsTrackerItemEntity> _goalsTrackerItemFactory;
    private readonly IGoalsTrackerItemsDao _goalsTrackerItemsDao;
    
    public GoalsTrackerItemsSeeder(
        IDataFactory<GoalsTrackerItemEntity> goalsTrackerItemFactory,
        IGoalsTrackerItemsDao goalsTrackerItemsDao
    )
    {
        _goalsTrackerItemFactory = goalsTrackerItemFactory;
        _goalsTrackerItemsDao = goalsTrackerItemsDao;
    }
    
    public async Task<GoalsTrackerItemEntity> CreateAsync(GoalsTrackerEntity tracker)
    {
        var fakeEntry = _goalsTrackerItemFactory.Generate();
        return await _goalsTrackerItemsDao.Create(
            tracker,
            fakeEntry.Name,
            fakeEntry.NumberOfTimes
        );
    }
    
    public async Task<ICollection<GoalsTrackerItemEntity>> CreateSeveralAsync(GoalsTrackerEntity tracker, int count = 1)
    {
        var result = new List<GoalsTrackerItemEntity>();
        for (int i = 0; i < count; i++)
        {
            result.Add(await CreateAsync(tracker));
        }

        return result;
    }
}
