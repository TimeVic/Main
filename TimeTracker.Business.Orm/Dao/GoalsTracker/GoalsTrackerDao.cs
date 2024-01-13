using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.GoalsTracker;

public class GoalsTrackerDao: IGoalsTrackerDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public GoalsTrackerDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<GoalsTrackerEntity> CheckAndCreate(UserEntity user, WorkspaceEntity workspace, DateTime date)
    {
        var tracker = await GetTracker(user, date);
        if (tracker == null)
        {
            tracker = new GoalsTrackerEntity()
            {
                Workspace = workspace,
                User = user,
                Year = date.Year,
                Month = date.Month,
                CreateTime = DateTime.UtcNow,
                UpdateTime = DateTime.UtcNow
            };
            await _sessionProvider.CurrentSession.SaveAsync(tracker);
        }

        return tracker;
    }
    
    public async Task<GoalsTrackerEntity?> GetById(long id)
    {
        return await _sessionProvider.CurrentSession.Query<GoalsTrackerEntity>()
            .Where(item => item.Id == id)
            .FirstOrDefaultAsync();
    }
    
    private async Task<GoalsTrackerEntity?> GetTracker(UserEntity user, DateTime date)
    {
        return await _sessionProvider.CurrentSession.Query<GoalsTrackerEntity>()
            .Where(item => item.Year == date.Year)
            .Where(item => item.Month == date.Month)
            .Where(item => item.User == user)
            .FirstOrDefaultAsync();
    }
}
