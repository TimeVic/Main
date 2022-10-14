using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface ITimeEntrySeeder: IDomainService
{
    Task<ICollection<TimeEntryEntity>> CreateSeveralAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        int count = 1,
        ProjectEntity? project = null
    );
}
