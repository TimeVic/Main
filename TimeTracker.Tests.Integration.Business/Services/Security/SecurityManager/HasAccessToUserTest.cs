using Autofac;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Security;
using TimeTracker.Business.Testing.Seeders.Entity;
using TimeTracker.Tests.Integration.Business.Core;

namespace TimeTracker.Tests.Integration.Business.Services.Security.SecurityManager;

public class HasAccessToUserTest: BaseTest
{
    private readonly ISecurityManager _securityManager;
    private readonly IUserSeeder _userSeeder;
    private readonly UserEntity _user;

    public HasAccessToUserTest(): base()
    {
        _securityManager = Scope.Resolve<ISecurityManager>();
        _userSeeder = Scope.Resolve<IUserSeeder>();
        _user = _userSeeder.CreateActivatedAsync().Result;
    }

    [Fact]
    public async Task ShouldHaveReadAccessToAnyUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Read, otherUser, _user);

        Assert.True(hasAccess);
    }

    [Fact]
    public async Task ShouldHaveWriteAccessToOwnUser()
    {
        var hasAccess = await _securityManager.HasAccess(AccessLevel.Write, _user, _user);

        Assert.True(hasAccess);
    }

    [Fact]
    public async Task ShouldNotHaveWriteAccessToOtherUser()
    {
        var otherUser = await _userSeeder.CreateActivatedAsync();

        var hasAccess = await _securityManager.HasAccess(AccessLevel.Write, otherUser, _user);

        Assert.False(hasAccess);
    }
}
