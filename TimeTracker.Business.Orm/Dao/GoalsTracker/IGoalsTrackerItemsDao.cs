using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.GoalsTracker;

public interface IGoalsTrackerItemsDao: IDomainService
{
    Task<GoalsTrackerItemEntity> Create(
        GoalsTrackerEntity goalsTracker,
        string name,
        int numberOfTimes = 0
    );
    
    Task<GoalsTrackerItemEntity?> GetById(long trackerItemId);

    Task<GoalsTrackerItemEntity> Update(
        GoalsTrackerItemEntity goalsTrackerItem,
        string name,
        int numberOfTimes = 0
    );

    Task<GoalsTrackerItemEntity> Archive(GoalsTrackerItemEntity item);

    Task SetCompletion(GoalsTrackerItemEntity goalsTrackerItem, int day, bool isCompleted);
}
