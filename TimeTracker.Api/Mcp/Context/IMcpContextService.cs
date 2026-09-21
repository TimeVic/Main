using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.Mcp.Context;

public interface IMcpContextService : IDomainService
{
    Task<UserEntity> GetUserAsync();
    
    Task<(UserEntity User, WorkspaceEntity Workspace)> GetUserAndWorkspaceAsync(Guid workspaceId);
}

