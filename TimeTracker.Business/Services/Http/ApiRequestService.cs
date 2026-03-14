using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Exceptions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Business.Services.Http;

public class ApiRequestService: BaseApiRequestService, IApiRequestService
{
    private readonly IUserDao _userDao;

    public ApiRequestService(
        IHttpContextAccessor httpContext,
        IJwtAuthService jwtAuthService,
        IUserDao userDao,
        IHttpTokenResolverService httpTokenResolverService
    ): base(httpContext, jwtAuthService, httpTokenResolverService)
    {
        _userDao = userDao;
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
