using Autofac;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Auth.AuthorizationService;

public class LoginTest: BaseTest
{
    private readonly IAuthorizationService _authService;
    private readonly IDataFactory<UserEntity> _userFactory;
    private readonly IUserSeeder _userSeeder;
    private readonly IJwtAuthService _jwtService;

    public LoginTest(): base()
    {
        _authService = Scope.Resolve<IAuthorizationService>();
        _userFactory = Scope.Resolve<IDataFactory<UserEntity>>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _jwtService = Scope.Resolve<IJwtAuthService>();
    }

    [Fact]
    public async Task ShouldLogin()
    {
        var expectedPassword = "some password";
        var user = _userSeeder.CreateActivatedAsync(expectedPassword).Result;
        
        await FlushDbChanges();
        var loginResponse = await _authService.Login(user.Email, expectedPassword);
        
        Assert.True(_jwtService.IsValidJwt(loginResponse.JwtToken));
        Assert.Equal(user.Id, _jwtService.GetUserId(loginResponse.JwtToken));
        
        var newAccessToken = await _authService.GenerateNewJwtToken(
            loginResponse.AccessToken,
            loginResponse.JwtToken
        );
        
        Assert.True(_jwtService.IsValidJwt(newAccessToken.JwtToken));
        Assert.Equal(user.Id, _jwtService.GetUserId(newAccessToken.JwtToken));
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfNotFound()
    {
        await FlushDbChanges();
        await Assert.ThrowsAsync<RecordNotFoundException>(async () =>
        {
            await _authService.Login("fake@email", "fake password");
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfIncorrectPassword()
    {
        var user = await _userSeeder.CreateActivatedAsync();
        await FlushDbChanges();
        await Assert.ThrowsAsync<UserNotAuthorizedException>(async () =>
        {
            await _authService.Login(user.Email, "fake 123 password");
        });
    }
}
