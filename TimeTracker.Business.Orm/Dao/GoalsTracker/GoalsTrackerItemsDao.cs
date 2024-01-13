using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.GoalsTracker;

public class GoalsTrackerItemsDao: IGoalsTrackerItemsDao
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IGoalsTrackerDao _goalsTrackerDao;

    public GoalsTrackerItemsDao(
        IDbSessionProvider sessionProvider,
        IGoalsTrackerDao goalsTrackerDao
    )
    {
        _sessionProvider = sessionProvider;
        _goalsTrackerDao = goalsTrackerDao;
    }
    
    public async Task<GoalsTrackerItemEntity?> GetById(long trackerItemId)
    {
        return await _sessionProvider.CurrentSession.Query<GoalsTrackerItemEntity>()
            .Where(item => item.Id == trackerItemId)
            .FirstOrDefaultAsync();
    }
    
    public async Task<GoalsTrackerItemEntity> Create(
        GoalsTrackerEntity goalsTracker,
        string name,
        int numberOfTimes = 0
    )
    {
        if (string.IsNullOrEmpty(name))
            throw new DataValidationException("Goal's name can not be empty");
        
        var trackerItem = new GoalsTrackerItemEntity()
        {
            Tracker = goalsTracker,
            Name = name,
            NumberOfTimes = numberOfTimes,
            IsArchived = false,
            UpdateTime = DateTime.UtcNow,
            CreateTime = DateTime.UtcNow
        };
        goalsTracker.Items.Add(trackerItem);
        await _sessionProvider.CurrentSession.SaveAsync(trackerItem);
        return trackerItem;
    }
    
    public async Task<GoalsTrackerItemEntity> Update(
        GoalsTrackerItemEntity goalsTrackerItem,
        string name,
        int numberOfTimes = 0
    )
    {
        if (string.IsNullOrEmpty(name))
            throw new DataValidationException("Goal's name can not be empty");

        goalsTrackerItem.Name = name;
        goalsTrackerItem.NumberOfTimes = numberOfTimes;
        goalsTrackerItem.UpdateTime = DateTime.UtcNow;
        await _sessionProvider.CurrentSession.SaveAsync(goalsTrackerItem);
        return goalsTrackerItem;
    }
    
    public async Task<GoalsTrackerItemEntity> Archive(GoalsTrackerItemEntity item)
    {
        item.IsArchived = true;
        item.UpdateTime = DateTime.UtcNow;
        await _sessionProvider.CurrentSession.SaveAsync(item);
        return item;
    }
    
    public async Task SetCompletion(GoalsTrackerItemEntity goalsTrackerItem, int day, bool isCompleted)
    {
        var completionMarker = await _sessionProvider.CurrentSession.Query<GoalsTrackerCompletionMarkerEntity>()
            .Where(item => item.GoalsTrackerItem == goalsTrackerItem)
            .Where(item => item.DayOfMonth == day)
            .FirstOrDefaultAsync();
        if (isCompleted)
        {
            if (completionMarker != null)
            {
                return;
            }

            completionMarker = new GoalsTrackerCompletionMarkerEntity()
            {
                CreateTime = DateTime.UtcNow,
                DayOfMonth = day,
                GoalsTrackerItem = goalsTrackerItem
            };
            goalsTrackerItem.CompletionMarkers.Add(completionMarker);
            await _sessionProvider.CurrentSession.SaveAsync(completionMarker);
            return;
        }
        if (completionMarker != null)
        {
            await _sessionProvider.CurrentSession.DeleteAsync(completionMarker);
        }  
    }
}
