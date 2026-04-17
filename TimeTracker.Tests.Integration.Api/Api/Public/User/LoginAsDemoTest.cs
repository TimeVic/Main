using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class LoginAsDemoTest : BaseTest
{
    private readonly string Url = "/user/login/as-demo";

    private readonly IJwtAuthService _jwtService;
    private readonly IUserAccessTokenDao _accessTokenDao;
    private readonly IUserDao _userDao;

    public LoginAsDemoTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();
        _userDao = ServiceProvider.GetRequiredService<IUserDao>();
    }

    [Fact]
    public async Task ShouldReturnValidAuthData()
    {
        var response = await GetRequestAsAnonymousAsync(Url);
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();

        Assert.True(_jwtService.IsValidJwt(responseData.JwtToken));
        Assert.NotEmpty(responseData.AccessToken);
        Assert.NotEqual(Guid.Empty, responseData.User.Id);
        Assert.NotEmpty(responseData.User.Email);
        Assert.NotNull(responseData.User.DefaultWorkspace);
        Assert.True(responseData.User.DefaultWorkspace.IsDefault);

        var actualAccessToken = await _accessTokenDao.GetByToken(responseData.AccessToken);
        Assert.NotNull(actualAccessToken);
        Assert.Contains(actualAccessToken.JwtTokens, item => item.Token == responseData.JwtToken);
    }

    [Fact]
    public async Task ShouldReturnDemoEmail()
    {
        var response = await GetRequestAsAnonymousAsync(Url);
        response.EnsureSuccessStatusCode();

        var responseData = await response.GetJsonDataAsync<LoginResponseDto>();

        Assert.True(DemoAccountConstants.IsDemoEmail(responseData.User.Email));
    }

    [Fact]
    public async Task ShouldReuseExistingDemoUserIfCreatedWithinWeek()
    {
        var response1 = await GetRequestAsAnonymousAsync(Url);
        response1.EnsureSuccessStatusCode();
        var data1 = await response1.GetJsonDataAsync<LoginResponseDto>();

        var response2 = await GetRequestAsAnonymousAsync(Url);
        response2.EnsureSuccessStatusCode();
        var data2 = await response2.GetJsonDataAsync<LoginResponseDto>();

        Assert.Equal(data1.User.Id, data2.User.Id);
        Assert.Equal(data1.User.Email, data2.User.Email);
    }

    [Fact]
    public async Task ShouldCreateNewDemoUserIfOlderThanWeek()
    {
        var response1 = await GetRequestAsAnonymousAsync(Url);
        response1.EnsureSuccessStatusCode();
        var data1 = await response1.GetJsonDataAsync<LoginResponseDto>();

        // Age the existing demo user beyond 7 days
        var demoUser = await _userDao.GetLastDemoUserAsync();
        Assert.NotNull(demoUser);
        demoUser.CreatedAt = DateTime.UtcNow.AddDays(-8);
        await FlushDbChanges();

        var response2 = await GetRequestAsAnonymousAsync(Url);
        response2.EnsureSuccessStatusCode();
        var data2 = await response2.GetJsonDataAsync<LoginResponseDto>();

        Assert.NotEqual(data1.User.Id, data2.User.Id);
        Assert.NotEqual(data1.User.Email, data2.User.Email);
    }
}

