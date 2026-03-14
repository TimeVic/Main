using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;
using TimeTracker.Business.Testing.Factories;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class SetNotificationTokenTest: BaseTest
{
    private readonly string Url = "/dashboard/user/set-notification-token";
    
    private readonly UserEntity _user;
    private readonly IDataFactory<WorkspaceEntity> _workspaceFactory;
    private readonly string _jwtToken;
    private readonly IDataFactory<UserNotificationTokenEntity> _notificationTokenFactory;
    private readonly IUserNotificationTokenDao _userNotificationTokenDao;

    public SetNotificationTokenTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _workspaceFactory = ServiceProvider.GetRequiredService<IDataFactory<WorkspaceEntity>>();
        _userNotificationTokenDao = ServiceProvider.GetRequiredService<IUserNotificationTokenDao>();
        _notificationTokenFactory = ServiceProvider.GetRequiredService<IDataFactory<UserNotificationTokenEntity>>();
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var token = _notificationTokenFactory.Generate();
        var response = await PostRequestAsAnonymousAsync(Url, new SetNotificationTokenRequest()
        {
            Token = token.Token,
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ShouldSet()
    {
        var expectedToken = _notificationTokenFactory.Generate();
        var response = await PostRequestAsync(Url, _jwtToken, new SetNotificationTokenRequest()
        {
            Token = expectedToken.Token,
        });
        response.EnsureSuccessStatusCode();

        var actualToken = await _userNotificationTokenDao.GetByToken(expectedToken.Token);
        Assert.NotNull(actualToken);
        Assert.Equal(expectedToken.Token, actualToken.Token);
        Assert.Equal(_user.Id, actualToken.User.Id);
    }
}
