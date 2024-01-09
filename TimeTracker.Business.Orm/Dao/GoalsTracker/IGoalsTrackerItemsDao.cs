using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.GoalsTracker;

public interface IGoalsTrackerItemsDao: IDomainService
{
    Task<GoalsTrackerItemEntity> Create(
        UserEntity user,
        int year,
        int month,
        string name,
        int numberOfTimes = 0
    );

    Task<GoalsTrackerItemEntity?> Get(long trackerItemId);

    Task<GoalsTrackerItemEntity> Update(
        GoalsTrackerItemEntity item,
        string name,
        int numberOfTimes = 0
    );

    Task<GoalsTrackerItemEntity> Archive(GoalsTrackerItemEntity item);

    Task SetCompletion(GoalsTrackerItemEntity goalsTrackerItem, int day, bool isCompleted);
}
