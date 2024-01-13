using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Dto.Auth;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Auth;

public class AuthorizationService: IAuthorizationService
{
    private readonly IUserDao _userDao;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IPasswordService _passwordService;
    private readonly IUserAccessTokenDao _accessTokenDao;
    private readonly IDbSessionProvider _sessionProvider;

    public AuthorizationService(
        IUserDao userDao,
        IJwtAuthService jwtAuthService,
        IPasswordService passwordService,
        IUserAccessTokenDao accessTokenDao,
        IDbSessionProvider sessionProvider
    )
    {
        _userDao = userDao;
        _jwtAuthService = jwtAuthService;
        _passwordService = passwordService;
        _accessTokenDao = accessTokenDao;
        _sessionProvider = sessionProvider;
    }

    public async Task<AuthResultDto> Login(string email, string password)
    {
        var user = await _userDao.GetByEmail(email);
        if (user is not { IsActivated: true })
        {
            throw new RecordNotFoundException();
        }
        if (!_passwordService.ValidatePassword(user, password))
        {
            throw new UserNotAuthorizedException();
        }

        return await Login(user);
    }
    
    public async Task<AuthResultDto> Login(UserEntity user)
    {
        var accessToken = await _accessTokenDao.CreateNew(user);
        return await GenerateNewJwtToken(accessToken.Token);
    }
    
    public async Task<AuthResultDto> GenerateNewJwtToken(string accessTokenString, string? previousJwtToken = null)
    {
        var accessToken = await _accessTokenDao.GetByToken(accessTokenString);
        if (
            accessToken == null
            || (
                !string.IsNullOrWhiteSpace(previousJwtToken)
                && accessToken.JwtTokens.All(item => item.Token != previousJwtToken)
            )
        )
        {
            throw new UserNotAuthorizedException();
        }

        if (accessToken.IsExpired)
        {
            await _accessTokenDao.Delete(accessToken);
            throw new ExpiredJwtTokenException();
        }
        
        var jwtToken = _jwtAuthService.BuildJwt(accessToken.User.Id);
        var jwtTokenEntity = new UserJwtTokenEntity()
        {
            Token = jwtToken,
            CreateTime = DateTime.UtcNow,
            ExpirationTime = JwtHelper.GetExpiryTimestamp(jwtToken),
            AccessToken = accessToken
        };
        accessToken.JwtTokens.Add(jwtTokenEntity);
        await _sessionProvider.CurrentSession.SaveAsync(jwtTokenEntity);
        
        return new AuthResultDto(
            jwtTokenEntity.Token,
            accessToken.Token,
            accessToken.User
        );
    }
}
