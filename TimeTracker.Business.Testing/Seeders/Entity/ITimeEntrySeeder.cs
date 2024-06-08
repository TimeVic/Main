using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface ITimeEntrySeeder: IDomainService
{
    Task<ICollection<TimeEntryEntity>> CreateSeveralAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        int count = 1,
        ProjectEntity? project = null
    );

    Task<TimeEntryEntity> CreateAsync(WorkspaceEntity workspace, UserEntity user, ProjectEntity? project = null);
}
