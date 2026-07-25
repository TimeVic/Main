using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Services.Workspace;

public interface IWorkspaceDeletionService : IDomainService
{
    Task SoftDeleteAsync(WorkspaceEntity workspace);

    Task HardDeleteAsync(WorkspaceEntity workspace, CancellationToken cancellationToken = default);
}
