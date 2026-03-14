using Autofac;
using Microsoft.AspNetCore.Http;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Helpers;
using TimeTracker.Business.Dto.Auth;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Services.Auth;

public class AuthorizationService: IAuthorizationService
{
    private readonly IUserDao _userDao;
    private readonly IJwtAuthService _jwtAuthService;
    private readonly IPasswordService _passwordService;
    private readonly IUserAccessTokenDao _accessTokenDao;
    private readonly IDbSessionProvider _sessionProvider;

    #region Scoped

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILifetimeScope _scope;
    private UserEntity? _loggedInUser;
    private Guid? _loggedInUserId;

    #endregion

    public AuthorizationService(
        IUserDao userDao,
        IJwtAuthService jwtAuthService,
        IPasswordService passwordService,
        IUserAccessTokenDao accessTokenDao,
        IDbSessionProvider sessionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILifetimeScope scope
    )
    {
        _userDao = userDao;
        _jwtAuthService = jwtAuthService;
        _passwordService = passwordService;
        _accessTokenDao = accessTokenDao;
        _sessionProvider = sessionProvider;
        _httpContextAccessor = httpContextAccessor;
        _scope = scope;
    }

    #region Get Authenticated User

    public async Task<UserEntity?> GetCurrentLoggedInUser()
    {
        var userGuid = GetCurrentLoggedInUserUid();
        if (!userGuid.HasValue)
            return null;
        if (userGuid != null)
        {
            _loggedInUser = await _userDao.GetById(userGuid.Value);
        }
        return _loggedInUser;
    }
    
    public Guid? GetCurrentLoggedInUserUid()
    {
        if (_loggedInUserId is not null)
            return _loggedInUserId;
        if (_scope.TryResolve(out IApiRequestService? apiRequestService))
        {
            var userGuid = apiRequestService.GetUserIdFromJwt();
            if (userGuid != Guid.Empty)
            {
                _loggedInUserId = userGuid;
            }    
        }
        return _loggedInUserId;
    }
    
    #endregion
    
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
        return await GenerateNewJwtToken(accessToken);
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
        return await GenerateNewJwtToken(accessToken);
    }
    
    public async Task<AuthResultDto> GenerateNewJwtToken(UserAccessTokenEntity accessToken)
    {
        if (accessToken.IsExpired)
        {
            await _accessTokenDao.Delete(accessToken);
            throw new ExpiredJwtTokenException();
        }
        
        var jwtToken = _jwtAuthService.BuildJwt(accessToken.User.Id);
        var jwtTokenEntity = new UserJwtTokenEntity()
        {
            Token = jwtToken,
            CreatedAt = DateTime.UtcNow,
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
