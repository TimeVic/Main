using System.Net;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Dashboard.User;

public class CheckLoginTest : BaseTest
{
    private const string Url = "/dashboard/user/check-login";

    private readonly string _jwtToken;
    private readonly UserEntity _user;

    public CheckLoginTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        (_jwtToken, _user, _) = UserSeeder.CreateAuthorizedAsync().Result;
    }

    [Fact]
    public async Task NonAuthorizedCanNotDoIt()
    {
        var response = await PostRequestAsAnonymousAsync(Url, new CheckLoginRequest
        {
            Login = "some_login"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShouldReturnAvailableForUniqueLogin()
    {
        var uniqueLogin = "avail_" + new Random().Next(10000, 99999);
        var response = await PostRequestAsync(Url, _jwtToken, new CheckLoginRequest
        {
            Login = uniqueLogin
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<CheckLoginResponse>();
        Assert.True(actualResponse.IsAvailable);
    }

    [Fact]
    public async Task ShouldReturnNotAvailableForTakenLogin()
    {
        var otherUser = await UserSeeder.CreateActivatedAsync();
        var response = await PostRequestAsync(Url, _jwtToken, new CheckLoginRequest
        {
            Login = otherUser.Login!
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<CheckLoginResponse>();
        Assert.False(actualResponse.IsAvailable);
    }

    [Fact]
    public async Task ShouldReturnAvailableForCurrentUserOwnLogin()
    {
        var response = await PostRequestAsync(Url, _jwtToken, new CheckLoginRequest
        {
            Login = _user.Login!
        });
        response.EnsureSuccessStatusCode();

        var actualResponse = await response.GetJsonDataAsync<CheckLoginResponse>();
        Assert.True(actualResponse.IsAvailable);
    }
}
