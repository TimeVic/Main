using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class LogoutTest : BaseTest
{
    private const string Url = "/user/logout";

    public LogoutTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldCleanUpAuthCookies()
    {
        var response = await PostRequestAsAnonymousAsync(Url);
        response.EnsureSuccessStatusCode();

        Assert.Equal(string.Empty, response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey()));
        Assert.Equal(string.Empty, response.GetSetCookieValue(HttpCookieKeyEnum.AccessToken.GetKey()));
    }
}
