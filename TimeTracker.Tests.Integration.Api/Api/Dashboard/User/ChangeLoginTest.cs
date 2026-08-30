using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class ChangeLoginTest : BaseTest
{
    private const string Url = "/dashboard/user/change-login";

    private readonly string _jwtToken;
    private readonly UserEntity _user;
    private readonly IUserDao _userDao;

    public ChangeLoginTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new ChangeLoginRequest
        {
            Login = "new_login"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldChangeLogin()
    {
        var newLogin = "updated_login_" + new Random().Next(1000, 9999);
        var response = await PostRequestAsync(Url, _jwtToken, new ChangeLoginRequest
        {
            Login = newLogin
        });
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        await FlushDbChanges(isClearSession: true);
        var persistedUser = await _userDao.GetById(_user.Id);

        Assert.Equal(newLogin, actualUser.Login);
        Assert.Equal(newLogin, persistedUser!.Login);
    }

    [Fact]
    public async Task ShouldChangeLoginWithLeadingAt()
    {
        var baseLogin = "at_login_" + new Random().Next(1000, 9999);
        var response = await PostRequestAsync(Url, _jwtToken, new ChangeLoginRequest
        {
            Login = "@" + baseLogin
        });
        response.EnsureSuccessStatusCode();

        var actualUser = await response.GetJsonDataAsync<UserDto>();
        await FlushDbChanges(isClearSession: true);
        var persistedUser = await _userDao.GetById(_user.Id);

        Assert.Equal(baseLogin, actualUser.Login);
        Assert.Equal(baseLogin, persistedUser!.Login);
    }

    [Fact]
    public async Task ShouldReturnBadRequestIfLoginAlreadyTaken()
    {
        var otherUser = await UserSeeder.CreateActivatedAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new ChangeLoginRequest
        {
            Login = otherUser.Login!
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal("RecordIsExistsException", responseData.ErrorCode);
    }
}
