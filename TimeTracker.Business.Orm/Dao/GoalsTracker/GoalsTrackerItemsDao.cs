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
    
    public async Task<GoalsTrackerCompletionMarkerEntity> SetCompletion(
        GoalsTrackerItemEntity goalsTrackerItem,
        int dayOfMonth,
        bool isChecked
    )
    {
        if (dayOfMonth > goalsTrackerItem.Tracker.DaysInCurrentMonth)
        {
            throw new DataValidationException("Invalid day number");
        }

        var existsMarked = await _sessionProvider.CurrentSession.Query<GoalsTrackerCompletionMarkerEntity>()
            .Where(item => item.GoalsTrackerItem == goalsTrackerItem)
            .Where(item => item.DayOfMonth == dayOfMonth)
            .FirstOrDefaultAsync();
        if (existsMarked == null)
        {
            existsMarked = new GoalsTrackerCompletionMarkerEntity()
            {
                DayOfMonth = dayOfMonth,
                GoalsTrackerItem = goalsTrackerItem,
                CreateTime = DateTime.UtcNow
            };
            goalsTrackerItem.CompletionMarkers.Add(existsMarked);
        }
        existsMarked.IsChecked = isChecked;
        existsMarked.UpdateTime = DateTime.UtcNow;
        await _sessionProvider.CurrentSession.SaveAsync(existsMarked);
        return existsMarked;
    }
    
    public async Task<GoalsTrackerItemEntity> Archive(GoalsTrackerItemEntity item)
    {
        item.IsArchived = true;
        item.UpdateTime = DateTime.UtcNow;
        await _sessionProvider.CurrentSession.SaveAsync(item);
        return item;
    }
}
