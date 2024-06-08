using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Seeders.Entity;

public interface ITagSeeder: IDomainService
{
    Task<ICollection<TagEntity>> CreateSeveralAsync(WorkspaceEntity workspace, int count = 1);

    Task<TagEntity> CreateAsync(WorkspaceEntity workspace);
}
