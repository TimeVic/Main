using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;

public interface IGoalsTrackerItemsSeeder: IDomainService
{
    Task<GoalsTrackerItemEntity> CreateAsync(GoalsTrackerEntity tracker);
    
    Task<ICollection<GoalsTrackerItemEntity>> CreateSeveralAsync(GoalsTrackerEntity tracker, int count = 1);
}
