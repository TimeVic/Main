using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.GoalsTracker;

public class GoalsTrackerDao: IGoalsTrackerDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public GoalsTrackerDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<GoalsTrackerEntity> CheckAndCreate(UserEntity user, int year, int month)
    {
        var tracker = await GetTracker(user, year, month);
        if (tracker == null)
        {
            tracker = new GoalsTrackerEntity()
            {
                User = user,
                Year = year,
                Month = month,
                CreateTime = DateTime.UtcNow
            };
            await _sessionProvider.CurrentSession.SaveAsync(tracker);
        }

        return tracker;
    }
    
    private async Task<GoalsTrackerEntity?> GetTracker(UserEntity user, int year, int month)
    {
        return await _sessionProvider.CurrentSession.Query<GoalsTrackerEntity>()
            .Where(item => item.Year == year)
            .Where(item => item.Month == month)
            .Where(item => item.User == user)
            .FirstOrDefaultAsync();
    }
}
