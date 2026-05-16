using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class JwtRefreshMiddlewareTest: BaseTest
{
    private const string Url = "/user/check-is-logged-in";

    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtAuthService _jwtService;

    public JwtRefreshMiddlewareTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _authorizationService = ServiceProvider.GetRequiredService<IAuthorizationService>();
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
    }

    [Fact]
    public async Task ShouldRefreshJwtTokenFromCookies()
    {
        var user = await UserSeeder.CreateActivatedAsync();
        var loginResponse = await _authorizationService.Login(user);
        await FlushDbChanges();

        // Wait 1+ second so the refreshed JWT has a different NotBefore/Expires timestamp
        await Task.Delay(1100);

        var request = new HttpRequestMessage(HttpMethod.Get, Url);
        request.Headers.Add(
            "Cookie",
            $"{HttpCookieKeyEnum.JwtToken.GetKey()}={loginResponse.JwtToken}; " +
            $"{HttpCookieKeyEnum.AccessToken.GetKey()}={loginResponse.AccessToken}"
        );

        var response = await HttpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var refreshedJwtToken = response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        Assert.NotEmpty(refreshedJwtToken);
        Assert.NotEqual(loginResponse.JwtToken, refreshedJwtToken);
        Assert.True(_jwtService.IsValidJwt(refreshedJwtToken!));
        Assert.Equal(user.Id, _jwtService.GetUserId(refreshedJwtToken!));
    }
}
