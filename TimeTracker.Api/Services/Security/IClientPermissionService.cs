using Domain.Abstractions;
using TimeTracker.Api.Shared.Constants;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.Services.Security;

public interface IClientPermissionService : IDomainService
{
    Task<ICollection<WorkspacePermission>> GetPermissionsAsync(
        UserEntity user,
        WorkspaceEntity workspace
    );
}
