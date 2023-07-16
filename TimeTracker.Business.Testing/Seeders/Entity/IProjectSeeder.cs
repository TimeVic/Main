using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface IProjectSeeder: IDomainService
{
    Task<ICollection<ProjectEntity>> CreateSeveralAsync(WorkspaceEntity workspace, int count = 1);

    Task<ProjectEntity> CreateAsync(WorkspaceEntity workspace, ClientEntity? client = null);
}
