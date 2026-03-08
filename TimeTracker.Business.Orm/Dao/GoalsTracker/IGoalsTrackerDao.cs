using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.GoalsTracker;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.GoalsTracker;

public interface IGoalsTrackerDao: IDomainService
{
    Task<GoalsTrackerEntity> CheckAndCreate(UserEntity user, WorkspaceEntity workspace, DateTime date);

    Task<GoalsTrackerEntity?> GetById(Guid id);
}
