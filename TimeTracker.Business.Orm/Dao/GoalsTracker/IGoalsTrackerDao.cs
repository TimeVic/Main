using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Orm.Dao.GoalsTracker;

public interface IGoalsTrackerDao: IDomainService
{
    Task<GoalsTrackerEntity> CheckAndCreate(UserEntity user, int year, int month);
}
