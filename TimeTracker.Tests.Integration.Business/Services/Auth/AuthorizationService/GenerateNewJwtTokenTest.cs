using Autofac;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Auth.AuthorizationService;

public class GenerateNewJwtTokenTest: BaseTest
{
    private readonly IAuthorizationService _authService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IJwtAuthService _jwtService;
    private readonly IUserAccessTokenDao _accessTokenDao;

    public GenerateNewJwtTokenTest(): base()
    {
        _authService = Scope.Resolve<IAuthorizationService>();
        _accessTokenDao = Scope.Resolve<IUserAccessTokenDao>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _jwtService = Scope.Resolve<IJwtAuthService>();
    }

    [Fact]
    public async Task ShouldGenerate()
    {
        var expectedPassword = "some password";
        var user = _userSeeder.CreateActivatedAsync(expectedPassword).Result;
        var loginResponse = await _authService.Login(user.Email, expectedPassword);
        
        // Act
        var newAccessToken = await _authService.GenerateNewJwtToken(
            loginResponse.AccessToken,
            loginResponse.JwtToken
        );
        Assert.True(newAccessToken.AccessToken.Length > 50);
        
        // Assert
        Assert.True(_jwtService.IsValidJwt(newAccessToken.JwtToken));
        Assert.Equal(user.Id, _jwtService.GetUserId(newAccessToken.JwtToken));
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfExpired()
    {
        var expectedPassword = "some password";
        var user = _userSeeder.CreateActivatedAsync(expectedPassword).Result;
        var loginResponse = await _authService.Login(user.Email, expectedPassword);
        loginResponse = await _authService.GenerateNewJwtToken(
            loginResponse.AccessToken,
            loginResponse.JwtToken
        );
        var accessToken = await _accessTokenDao.GetByToken(loginResponse.AccessToken);
        Assert.NotNull(accessToken);
        accessToken.ExpirationTime = DateTime.UtcNow.AddSeconds(-1);
        await CommitDbChanges();
        
        await Assert.ThrowsAsync<ExpiredJwtTokenException>(async () =>
        {
            await _authService.GenerateNewJwtToken(
                loginResponse.AccessToken,
                loginResponse.JwtToken
            );
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfIncorrectJwt()
    {
        var expectedPassword = "some password";
        var user = _userSeeder.CreateActivatedAsync(expectedPassword).Result;
        var loginResponse = await _authService.Login(user.Email, expectedPassword);
        loginResponse = await _authService.GenerateNewJwtToken(
            loginResponse.AccessToken,
            loginResponse.JwtToken
        );
        var accessToken = await _accessTokenDao.GetByToken(loginResponse.AccessToken);
        accessToken.ExpirationTime = DateTime.UtcNow.AddSeconds(-1);
        await CommitDbChanges();
        
        await Assert.ThrowsAsync<UserNotAuthorizedException>(async () =>
        {
            await _authService.GenerateNewJwtToken(
                loginResponse.AccessToken,
                loginResponse.JwtToken + "1"
            );
        });
    }
}
