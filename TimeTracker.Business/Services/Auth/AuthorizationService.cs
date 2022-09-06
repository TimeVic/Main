using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Auth;

public class AuthorizationService: IAuthorizationService
{
    private readonly IUserDao _userDao;
    private readonly IJwtAuthService _jwtAuthService;

    public AuthorizationService(IUserDao userDao, IJwtAuthService jwtAuthService)
    {
        _userDao = userDao;
        _jwtAuthService = jwtAuthService;
    }

    public async Task<(string token, UserEntity user)> Login(string email, string password)
    {
        var user = await _userDao.GetByEmail(email);
        if (user is not { IsActivated: true })
        {
            throw new RecordNotFoundException();
        }
        var passwordHash = SecurityUtil.GeneratePasswordHash(password, user.PasswordSalt);
        var isLoggedIn = user.PasswordHash.CompareTo(passwordHash);
        if (!isLoggedIn)
        {
            throw new UserNotAuthorizedException();
        }

        return (
            _jwtAuthService.BuildJwt(user.Id),
            user
        );
    }
}
