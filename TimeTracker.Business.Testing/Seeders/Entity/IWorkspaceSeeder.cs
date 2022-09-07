using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IWorkspaceSeeder: IDomainService
{
    Task<ICollection<WorkspaceEntity>> CreateSeveralAsync(UserEntity user, int count = 1);
}
