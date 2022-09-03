using Autofac;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Exceptions.Api;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
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
        
        var (jwtToken, _) = await _authService.Login(user.Email, expectedPassword);
        
        Assert.True(_jwtService.IsValidJwt(jwtToken));
        Assert.Equal(user.Id, _jwtService.GetUserId(jwtToken));
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfNotFound()
    {
        await Assert.ThrowsAsync<RecordNotFoundException>(async () =>
        {
            await _authService.Login("fake@email", "fake password");
        });
    }
    
    [Fact]
    public async Task ShouldThrowExceptionIfIncorrectPassword()
    {
        var user = _userSeeder.CreateActivatedAsync().Result;
        await Assert.ThrowsAsync<UserNotAuthorizedException>(async () =>
        {
            await _authService.Login(user.Email, "fake 123 password");
        });
    }
}
