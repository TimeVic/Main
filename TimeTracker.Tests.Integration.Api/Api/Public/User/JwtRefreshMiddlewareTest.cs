using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NHibernate.Linq;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Testing.Extensions;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class JwtRefreshMiddlewareTest: BaseTest
{
    private const string Url = "/user/check-is-logged-in";

    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtAuthService _jwtService;
    private readonly string _cookieKeyPostfix;

    public JwtRefreshMiddlewareTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _authorizationService = ServiceProvider.GetRequiredService<IAuthorizationService>();
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _cookieKeyPostfix = ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("App:Auth:CookieKeyPostfix") ?? string.Empty;
    }

    [Fact]
    public async Task ShouldRefreshJwtTokenFromCookies()
    {
        var user = await UserSeeder.CreateActivatedAsync();
        var loginResponse = await _authorizationService.Login(user);
        await FlushDbChanges();

        var accessToken = await DbSessionProvider.CurrentSession.Query<UserAccessTokenEntity>()
            .SingleAsync(item => item.Token == loginResponse.AccessToken);
        var expirationTime = DateTime.UtcNow.AddSeconds(10);
        var expiringJwtToken = _jwtService.BuildJwt(
            user.Id,
            accessToken.Id,
            expirationTime
        );
        await DbSessionProvider.CurrentSession.SaveAsync(new UserJwtTokenEntity
        {
            Token = expiringJwtToken,
            ExpirationTime = expirationTime,
            CreatedAt = DateTime.UtcNow,
            AccessToken = accessToken
        });
        await FlushDbChanges();

        var request = new HttpRequestMessage(HttpMethod.Get, Url);
        var jwtCookieName = PrepareCookieName(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessTokenCookieName = PrepareCookieName(HttpCookieKeyEnum.AccessToken.GetKey());
        request.Headers.Add(
            "Cookie",
            $"{jwtCookieName}={expiringJwtToken}; " +
            $"{accessTokenCookieName}={loginResponse.AccessToken}"
        );

        var response = await HttpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var refreshedJwtToken = response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey());
        Assert.NotEmpty(refreshedJwtToken);
        Assert.NotEqual(expiringJwtToken, refreshedJwtToken);
        Assert.True(_jwtService.IsValidJwt(refreshedJwtToken!));
        Assert.Equal(user.Id, _jwtService.GetUserId(refreshedJwtToken!));
    }

    [Fact]
    public async Task ShouldNotRefreshFreshJwtToken()
    {
        var user = await UserSeeder.CreateActivatedAsync();
        var loginResponse = await _authorizationService.Login(user);
        await FlushDbChanges();

        var request = new HttpRequestMessage(HttpMethod.Get, Url);
        var jwtCookieName = PrepareCookieName(HttpCookieKeyEnum.JwtToken.GetKey());
        var accessTokenCookieName = PrepareCookieName(HttpCookieKeyEnum.AccessToken.GetKey());
        request.Headers.Add(
            "Cookie",
            $"{jwtCookieName}={loginResponse.JwtToken}; " +
            $"{accessTokenCookieName}={loginResponse.AccessToken}"
        );

        var response = await HttpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Null(response.GetSetCookieValue(HttpCookieKeyEnum.JwtToken.GetKey()));
    }

    private string PrepareCookieName(string baseName)
    {
        if (string.IsNullOrEmpty(_cookieKeyPostfix))
        {
            return baseName;
        }

        return $"{baseName}_{_cookieKeyPostfix}";
    }
}
