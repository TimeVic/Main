using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity.GoalsTracker;

public interface IGoalsTrackerSeeder: IDomainService
{
    Task<ICollection<GoalsTrackerEntity>> CreateSeveralAsync(UserEntity user, WorkspaceEntity workspace, int count = 1);
    
    Task<ICollection<GoalsTrackerEntity>> CreateSeveralAsync(int count = 1);

    Task<GoalsTrackerEntity> CreateAsync(UserEntity user, WorkspaceEntity workspace, int count = 1);
}
