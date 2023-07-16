using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IWorkspaceSeeder: IDomainService
{
    Task<ICollection<WorkspaceEntity>> CreateSeveralAsync(UserEntity user, int count = 1);
}
