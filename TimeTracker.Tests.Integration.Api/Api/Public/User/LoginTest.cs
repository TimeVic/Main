using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Common.Extensions;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Tests.Integration.Api.Core;

namespace TimeTracker.Tests.Integration.Api.Api.Public.User;

public class LoginTest: BaseTest
{
    private readonly string Url = "/user/login";
    
    private readonly IJwtAuthService _jwtService;
    private readonly IUserAccessTokenDao _accessTokenDao;

    public LoginTest(ApiCustomWebApplicationFactory factory) : base(factory)
    {
        _jwtService = ServiceProvider.GetRequiredService<IJwtAuthService>();
        _accessTokenDao = ServiceProvider.GetRequiredService<IUserAccessTokenDao>();
    }

    [Fact]
    public async Task ShouldLogin()
    {
        var expectedPassword = "test password";
        var user = await UserSeeder.CreateActivatedAsync(expectedPassword);
        var response = await PostRequestAsAnonymousAsync(Url, new LoginRequest()
        {
            Email = user.Email,
            Password = expectedPassword,
            ReCaptcha = "captcha"
        });
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
    public async Task ShouldFailIfIncorrectPassword()
    {
        var user = await UserSeeder.CreateActivatedAsync();
        var response = await PostRequestAsAnonymousAsync(Url, new LoginRequest()
        {
            Email = user.Email,
            Password = "some incorrect password",
            ReCaptcha = "captcha"
        });
        var responseData = await response.GetJsonResponseAsync<object>();
        Assert.Equal(new UserNotAuthorizedException().GetTypeName(), responseData.ErrorCode);
    }
}
