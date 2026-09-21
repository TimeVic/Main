using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Api.Mcp.Context;

public class McpContextService : IMcpContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserDao _userDao;

    public McpContextService(
        IHttpContextAccessor httpContextAccessor,
        IUserDao userDao
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _userDao = userDao;
    }

    public async Task<UserEntity> GetUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new HasNoAccessException();
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new HasNoAccessException();
        }

        var user = await _userDao.GetById(userId);
        if (user == null || user.DeletedAt != null || !user.IsActivated)
        {
            throw new HasNoAccessException();
        }

        return user;
    }

    public async Task<(UserEntity User, WorkspaceEntity Workspace)> GetUserAndWorkspaceAsync(Guid workspaceId)
    {
        var user = await GetUserAsync();
        var workspace = await _userDao.GetUsersWorkspace(user, workspaceId);
        RecordNotFoundException.ThrowIfNull(workspace, "Workspace not found");

        return (user, workspace);
    }
}

