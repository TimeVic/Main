using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Common.Exceptions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Business.Services.Http;

public class ApiRequestService: BaseApiRequestService, IApiRequestService
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly IUserDao _userDao;

    public ApiRequestService(
        IHttpContextAccessor httpContext,
        IJwtAuthService jwtAuthService,
        IUserDao userDao,
        IHttpTokenResolverService httpTokenResolverService
    ): base(httpContext, jwtAuthService, httpTokenResolverService)
    {
        _httpContext = httpContext;
        _userDao = userDao;
    }

    public Guid? GetCurrentWorkspaceId()
    {
        var rawWorkspaceId = _httpContext.HttpContext?.Request.Headers[AuthConstants.WorkspaceIdHeaderName].FirstOrDefault();
        return Guid.TryParse(rawWorkspaceId, out var workspaceId) ? workspaceId : null;
    }
    
    public async Task<UserEntity> GetCurrentUser()
    {
        var guid = GetCurrentUserId();
        var user = await _userDao.GetById(guid);
        if (user == null || user.DeletedAt != null)
        {
            throw DomainException.UserNotFoundException;
        }
        return user;
    }
    
    public async Task<UserEntity?> GetCurrentUserOrNull()
    {
        var guid = GetUserGuidFromJwt();
        if (guid == null)
            return null;

        var user = await _userDao.GetById(guid.Value);
        if (user == null || user.DeletedAt != null)
        {
            return null;
        }
        return user;
    }
}
